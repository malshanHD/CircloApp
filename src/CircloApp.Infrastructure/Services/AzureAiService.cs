#pragma warning disable OPENAI001

using CircloApp.Application.Features.AI.DTO;
using CircloApp.Application.Features.AI.Queries.GetEventAiAnalysis;
using CircloApp.Application.Features.Expenses.DTOs;
using Microsoft.Extensions.Options;
using OpenAI.Responses;
using System.ClientModel;
using System.Text.Json;

namespace CircloApp.Infrastructure.Services
{
    public class AzureAiService 
    {
        private readonly ResponsesClient _responsesClient;
        private readonly AzureAIOptions _azureAIOptions;

        public AzureAiService(IOptions<AzureAIOptions> options)
        {
            _azureAIOptions = options.Value;

            _responsesClient = new ResponsesClient(
            new ApiKeyCredential(_azureAIOptions.ApiKey),
            new ResponsesClientOptions
            {
                Endpoint = new Uri(_azureAIOptions.Endpoint)
            });
        }

        public Task<string> AskCircloAsync(Guid eventId, string question, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> AskWithRagAsync(Guid eventId, string question, CancellationToken cancelToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<CategorizedExpensesResponse> CategorizedExpensesAsync(List<EventExpensesResponnse> expenses, CancellationToken cancellationToken = default)
        {
            var expenseText = string.Join(
            "\n",
            expenses.Select(e =>
                $"ExpenseId: {e.Id}, Description: {e.Description}"));

            var prompt = $$"""
                        You are the expense categorization engine for Circlo.

                        Categorize every provided expense.

                        Use the expenseId exactly as provided.
                        Do not omit any expenses.

                        Expenses:
                        {{expenseText}}
                        """;

            var schema = BinaryData.FromString("""
            {
              "type": "object",
              "properties": {
                "expenses": {
                  "type": "array",
                  "items": {
                    "type": "object",
                    "properties": {
                      "expenseId": {
                        "type": "string"
                      },
                      "category": {
                        "type": "string",
                        "enum": [
                          "Food",
                          "Transport",
                          "Accommodation",
                          "Entertainment",
                          "Shopping",
                          "Utilities",
                          "Other"
                        ]
                      }
                    },
                    "required": [
                      "expenseId",
                      "category"
                    ],
                    "additionalProperties": false
                  }
                }
              },
              "required": [
                "expenses"
              ],
              "additionalProperties": false
            }
            """);

            var options = new CreateResponseOptions
            {
                Model = _azureAIOptions.DeploymentName
            };

            options.InputItems.Add(ResponseItem.CreateUserMessageItem(prompt));

            options.TextOptions = new ResponseTextOptions
            {
                TextFormat = ResponseTextFormat.CreateJsonSchemaFormat("circlo_expense_categories", schema, jsonSchemaIsStrict: true)
            };

            var response = await _responsesClient.CreateResponseAsync(options, cancellationToken);

            var json = response.Value.GetOutputText();

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidOperationException(
                    "AI returned an empty categorization response.");
            }

            var result = JsonSerializer.Deserialize<CategorizedExpensesResponse>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (result is null)
            {
                throw new InvalidOperationException(
                    "Unable to deserialize AI categorization response.");
            }

            return result;
        }

        public async Task<string> GenerateAsync(string prompt,CancellationToken cancellationToken = default)
        {
                var inputItems = new List<ResponseItem>
            {
                ResponseItem.CreateUserMessageItem(prompt)
            };

            var response = await _responsesClient.CreateResponseAsync(
                _azureAIOptions.DeploymentName,
                inputItems,
                previousResponseId: null,
                cancellationToken: cancellationToken);

            return response.Value.GetOutputText();
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

            var options = new CreateResponseOptions
            {
                Model = _azureAIOptions.DeploymentName
            };

            options.InputItems.Add(ResponseItem.CreateUserMessageItem(prompt));

            var response = await _responsesClient.CreateResponseAsync(options, cancellationToken);

            return response.Value.GetOutputText();
        }
    }
}

#pragma warning restore OPENAI001