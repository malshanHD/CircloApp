using CircloApp.Application.Features.AI.DTO;
using CircloApp.Application.Features.AI.Queries.GetEventAiAnalysis;
using CircloApp.Application.Features.Expenses.DTOs;
using CircloApp.Application.Interfaces;
using CircloApp.Infrastructure.AI.Plugins;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;

namespace CircloApp.Infrastructure.Services
{
    public class SemanticKernelAiService : IAiService
    {
        private readonly Kernel _kernal;
        private readonly AzureAIOptions _azureAIOptions;

        public SemanticKernelAiService(IOptions<AzureAIOptions> options, CircloExpensePlugin expensePlugin)
        {
            _azureAIOptions = options.Value;
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
