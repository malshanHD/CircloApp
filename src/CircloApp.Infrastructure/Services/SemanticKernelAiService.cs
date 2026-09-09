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
            var intent = await ClassifyQuestionAsync(question,cancellationToken);

            Console.WriteLine($"Detected intent: {intent}");

            return intent switch
            {
                AiQuestionIntent.Structured =>
                    await AskCircloAsync(
                        eventId,
                        question,
                        cancellationToken),

                AiQuestionIntent.Sementic =>
                    await AskWithRagAsync(
                        eventId,
                        question,
                        cancellationToken),

                AiQuestionIntent.SemanticCalculation =>
                    await AskSemanticCalculationAsync(
                        eventId,
                        question,
                        cancellationToken),

                _ => throw new InvalidOperationException(
                    "Unsupported AI intent.")
            };
        }

        public async Task<string> AskSemanticCalculationAsync(Guid eventId, string question, CancellationToken cancellationToken)
        {
            var searchResult = await _expenseVectorSearchService.SearchExpenseAsync(eventId, question, cancellationToken);

            if (searchResult.Count == 0)
            {
                return "I couldn't find any relevant expenses.";
            }

            var expenseIds = searchResult.Select(x => Guid.Parse(x.Id)).ToList();

            var expenses = await _expensesService.GetExpensesByIdsAsync(eventId, expenseIds, cancellationToken);

            if (expenses.Count == 0)
            {
                return "I couldn't find the matching expenses.";
            }

            var total = expenses.Sum(x => x.Amount);

            var contextBuilder = new StringBuilder();

            foreach (var expense in expenses)
            {
                contextBuilder.AppendLine($"- {expense.Description}: {expense.Amount}");
            }

            contextBuilder.AppendLine();
            contextBuilder.AppendLine($"Application-calculated total: {total}");

            var prompt = """
                        You are the AI expense assistant for the Circlo application.

                        The application has already identified expenses that may be relevant
                        to the user's question and has loaded the authoritative expense data
                        from the database.

                        The application has also calculated the total using C#.

                        Your job is to explain the result clearly to the user.

                        Rules:
                        - Use ONLY the information provided in the expense context.
                        - Do not invent expenses, amounts, categories, people, or other facts.
                        - Do not recalculate the total yourself.
                        - Use the application-calculated total when a total is required.
                        - If the supplied expenses do not provide enough information to answer
                          the question, clearly say so.
                        - Keep the answer concise and easy to understand.

                        Expense context:
                        {{$context}}

                        User question:
                        {{$question}}
                        """;

            var arguments = new KernelArguments
            {
                ["context"] = contextBuilder.ToString(),
                ["question"] = question
            };

            var result = await _kernal.InvokePromptAsync(prompt, arguments, cancellationToken: cancellationToken);

            return result.ToString();
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

                        Answer the user's question using ONLY the expense information
                        provided below.

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

        public async Task<AiQuestionIntent> ClassifyQuestionAsync(string question, CancellationToken cancellationToken = default)
        {
            var prompt = """
                            Classify the Circlo expense question into exactly ONE of these values:

                            Structured
                            Semantic
                            SemanticCalculation

                            Rules:

                            Structured:
                            Use when the question can be answered directly from structured
                            database facts without semantically identifying a subset of expenses.

                            Examples:
                            - Who paid the most?
                            - How much did Malshan pay?
                            - How much did each person pay?
                            - What is the total cost of the event?

                            Semantic:
                            Use when expenses must be identified by meaning, description,
                            category, or semantic relationship, and no calculation is requested.

                            Examples:
                            - What food expenses did we have?
                            - Show transport-related expenses.
                            - What did we buy for breakfast?
                            - Find accommodation expenses.

                            SemanticCalculation:
                            Use when expenses must first be identified semantically
                            and then an exact calculation, aggregation, comparison,
                            maximum, minimum, or total is requested.

                            Examples:
                            - How much did we spend on food?
                            - What was the total transport cost?
                            - How much did breakfast cost?
                            - Who paid the most for transport?
                            - What was the largest food expense?

                            Important:
                            If the question contains a semantic concept such as food,
                            breakfast, transport, accommodation, hotel, travel,
                            lunch, dinner, etc. AND asks for a total, amount,
                            comparison, maximum, minimum, or aggregation,
                            choose SemanticCalculation.

                            Question:
                            {{$question}}
                            """;

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ResponseFormat = typeof(AiQuestionClassification)
            };

            var arguments = new KernelArguments(executionSettings)
            {
                ["question"] = question
            };

            var result = await _kernal.InvokePromptAsync(
                prompt,
                arguments,
                cancellationToken: cancellationToken);

            var classification =
                JsonSerializer.Deserialize<AiQuestionClassification>(
                    result.ToString());

            if (classification is null)
                throw new InvalidOperationException(
                    "Unable to classify question.");

            return classification.Intent.Trim() switch
            {
                "Structured" =>
                    AiQuestionIntent.Structured,

                "Semantic" =>
                    AiQuestionIntent.Sementic,

                "SemanticCalculation" =>
                    AiQuestionIntent.SemanticCalculation,

                _ => throw new InvalidOperationException(
                    $"Unknown AI intent: {classification.Intent}")
            };
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
