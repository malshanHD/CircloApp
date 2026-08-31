using CircloApp.Application.Features.AI.DTO;
using CircloApp.Application.Features.AI.Queries.GetEventAiAnalysis;
using CircloApp.Application.Features.Expenses.DTOs;
using CircloApp.Application.Interfaces;
using CircloApp.Infrastructure.AI.Plugins;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CircloApp.Infrastructure.Services
{
    public class SemanticKernelAiService : IAiService
    {
        private readonly Kernel _kernal;
        private readonly AzureAIOptions _azureAIOptions;
        private readonly IExpenseVectorSearchService _expenseVectorSearchService;
        private readonly IExpensesService _expensesService;

        public SemanticKernelAiService(IOptions<AzureAIOptions> options, CircloExpensePlugin expensePlugin, 
                                       IExpenseVectorSearchService expenseVectorSearchService, IExpensesService expensesService)
        {
            _azureAIOptions = options.Value;
            _expenseVectorSearchService = expenseVectorSearchService;
            _expensesService = expensesService;

            var builder = Kernel.CreateBuilder();

            builder.AddAzureOpenAIChatCompletion(deploymentName: _azureAIOptions.DeploymentName, endpoint: _azureAIOptions.SemanticKernelEndpoint, apiKey: _azureAIOptions.ApiKey);

            _kernal = builder.Build();

            _kernal.Plugins.AddFromObject(expensePlugin, "CircloExpenses");
        }

        public async Task<string> AskCircloAsync(Guid eventId, string question, CancellationToken cancellationToken = default)
        {
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            var prompt = $$""""
                         You are the AI assistant for Circlo, a shared expense application.

                         answer questions about the specified event using the available tools.

                         Event ID:
                         {{eventId}}

                         User Question:
                         {{question}}

                         Important rules:
                         - Use tools when event-specific information is required.
                         - Never invent event data.
                         - Only answer using information returned by the available tools.
                         - Keep the answer concise.
                         """";

            var result = await _kernal.InvokePromptAsync(prompt, new KernelArguments(executionSettings), cancellationToken: cancellationToken);

            return result.ToString();
        }

        public async Task<string> AskCircloSmartAsync(Guid eventId, string question, CancellationToken cancellationToken = default)
        {
            var classification = await ClassifyQuestionAsync(question, cancellationToken);

            return classification.Intent switch
            {
                AiQuestionIntent.Structured => await AskCircloAsync(
                eventId,
                question,
                cancellationToken),

                AiQuestionIntent.Sementic => await AskWithRagAsync(
                        eventId,
                        question,
                        cancellationToken),

                _ => throw new InvalidOperationException("Unsupported AI question intent.")
            };
        }

        public async Task<string> AskWithRagAsync(Guid eventId, string question, CancellationToken cancelToken = default)
        {
            var searchResult = await _expenseVectorSearchService.SearchExpenseAsync(eventId, question, cancelToken);

            if (searchResult.Count == 0)
            {
                return "I couldn't find any relevant expense information for this event.";
            }

            var expenseIds = searchResult.Select(x => Guid.Parse(x.Id)).ToList();

            var expenses = await _expensesService.GetExpensesByIdsAsync(eventId, expenseIds, cancelToken);

            var total = expenses.Sum(x => x.Amount);

            var contextBuilder = new StringBuilder();

            foreach (var expense in expenses)
            {
                contextBuilder.AppendLine($"- {expense.Description}: {expense.Amount}");
            }

            contextBuilder.AppendLine();
            contextBuilder.AppendLine($"Calculated total: {total}");

            var context = contextBuilder.ToString();

            var prompt = """
                            You are an expense assistant for the Circlo application.

                            Answer the user's question using ONLY the information
                            provided in the context.

                            The monetary calculations in the context were already
                            performed by the application.

                            Do not recalculate totals.
                            Do not invent expenses, amounts, categories, or facts.

                            If the context is not sufficient to answer the question,
                            say that there is not enough information.

                            Expense context:
                            {{$context}}

                            User question:
                            {{$question}}

                            Give a concise and helpful answer.
                            """;

            var arguments = new KernelArguments
            {
                ["context"] = context,
                ["question"] = question
            };

            var result = await _kernal.InvokePromptAsync(prompt, arguments, cancellationToken: cancelToken);

            return result.ToString();
        }

        public async Task<CategorizedExpensesResponse> CategorizedExpensesAsync(List<EventExpensesResponnse> expenses, CancellationToken cancellationToken = default)
        {
            var expenseText = string.Join("\n", expenses.Select(e => $"ExpenseId: {e.Id}, Description: {e.Description}"));

            var prompt = $$""""
                         You are the expense categorization engine for Circlo.

                         Categorize every provided expense into exactly one of these categories:

                         - Food
                         - Transport
                         - Accommodation
                         - Entertainment
                         - Shopping
                         - Utilities
                         - Other

                         Use each expenseId exactly as provided.
                         Do no omit any expenses.

                         Expenses:
                         {{expenseText}}
                         """";

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ResponseFormat = typeof(CategorizedExpensesResponse)
            };

            var result = await _kernal.InvokePromptAsync(prompt, new KernelArguments(executionSettings), cancellationToken: cancellationToken);

            var json = result.ToString();

            var response = JsonSerializer.Deserialize<CategorizedExpensesResponse>(
                           json,
                           new JsonSerializerOptions
                           {
                               PropertyNameCaseInsensitive = true,
                           });

            if (response is null)
            {
                throw new InvalidOperationException("Unable to deserialize AI categorization response.");
            }

            return response;
        }

        public async Task<AiQuestionClassification> ClassifyQuestionAsync(string question, CancellationToken cancellationToken = default)
        {
            var prompt = """
                            You classify questions for the Circlo expense application.

                            Choose exactly one intent:

                            Structured:
                            Questions that require exact database facts, calculations,
                            member information, totals, balances, who paid, how much someone paid,
                            highest/lowest spending, or other structured numerical information.

                            Semantic:
                            Questions that ask about the meaning or type of expenses,
                            such as food-related expenses, transport-related expenses,
                            accommodation expenses, or finding expenses based on descriptions.

                            Examples:

                            "Who paid the most?"
                            Structured

                            "How much did John pay?"
                            Structured

                            "What food expenses did we have?"
                            Semantic

                            "Show me travel-related expenses."
                            Semantic

                            Question:
                            {{$question}}
                            """;

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ResponseFormat = typeof(AiQuestionClassification)
            };

            var argument = new KernelArguments(executionSettings)
            {
                ["question"] = question
            };

            var result = await _kernal.InvokePromptAsync(prompt, argument, cancellationToken: cancellationToken);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            options.Converters.Add(new JsonStringEnumConverter());

            var classification = JsonSerializer.Deserialize<AiQuestionClassification>(result.ToString(), options);

            return classification ?? throw new InvalidOperationException("Unable to classify AI question");
        }

        public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
        {
            var result = await _kernal.InvokePromptAsync(prompt, cancellationToken: cancellationToken);

            return result.ToString();
        }

        public async Task<string> GenerateExpenseSummaryAsync(decimal totalExpense, List<CategorySummaryDto> categories, CancellationToken cancellationToken = default)
        {
            var categoryText = string.Join(
                "\n",
                categories.Select(x => $"{x.Category}: {x.Amount} ({x.Percentage}%)"));

            var prompt = $$"""
                            You are an expense analysis assistant for Circlo.

                            Write a short, useful summary of this event's spending.

                            Total expense: {{totalExpense}}

                            Categories:
                            {{categoryText}}

                            Requirements:
                            - Use only the numbers provided.
                            - Do not calculate or invent additional amounts.
                            - Mention the most significant spending category.
                            - Keep the response to 2 or 3 sentences.
                            - Do not use markdown.
                            """;

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ResponseFormat = typeof(CategorizedExpensesResponse)
            };

            var result = await _kernal.InvokePromptAsync(prompt, new KernelArguments(executionSettings), cancellationToken: cancellationToken);

            return result.ToString();
        }
    }
}
