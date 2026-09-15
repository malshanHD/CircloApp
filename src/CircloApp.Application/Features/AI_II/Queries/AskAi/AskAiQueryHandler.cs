using CircloApp.Application.Exceptions;
using CircloApp.Application.Features.AI_II.Models;
using CircloApp.Application.Features.Expenses.DTOs;
using CircloApp.Application.Interfaces;
using CircloApp.Application.Interfaces.AI;
using MediatR;
using System.Security.AccessControl;
using System.Text;

namespace CircloApp.Application.Features.AI_II.Queries.AskAi
{
    public class AskAiQueryHandler : IRequestHandler<AskAiQuery, string>
    {
        private readonly IAiChatService _chatService;
        private readonly IEventMemberRepository _members;
        private readonly ICurrentUserService _currentUser;
        private readonly IExpenseSearchService _expenseSearchService;
        private readonly IAiIntentService _aiIntentService;
        private readonly IExpenseRelevanceService _expenseRelevanceService;
        private readonly IExpensesService _expenseRepository;

        public AskAiQueryHandler(IAiChatService chatService, IExpenseSearchService expenseSearchService, 
                                 IAiIntentService aiIntentService, IExpenseRelevanceService expenseRelevanceService, IEventMemberRepository members, ICurrentUserService currentUser,
                                 IExpensesService expenseRepository)
        {
            _chatService = chatService;
            _members = members;
            _currentUser = currentUser;
            _expenseSearchService = expenseSearchService;
            _aiIntentService = aiIntentService;
            _expenseRelevanceService = expenseRelevanceService;
            _expenseRepository = expenseRepository;
        }

        public async Task<string> Handle(AskAiQuery request, CancellationToken cancellationToken)
        {
            var member = await _members.GetEventMember(request.EventId, _currentUser.UserId, cancellationToken);
            if (member is null || !member.IsActive || member.IsDeleted)
                throw new BadRequestException("Event not found");
            if (string.IsNullOrWhiteSpace(request.Question))
                throw new BadRequestException("Enter a question.");

            var analysis = await _aiIntentService.AnalyzeAsync(request.Question, cancellationToken);

            if (analysis.Intent == AiQueryIntent.SemanticCalculation)
            {
                return await HandleSemanticCalculationAsync(request.EventId, request.Question, cancellationToken);
            }

            if (analysis.Intent == AiQueryIntent.StructuredCalculation)
            {
                return await HandleStructuredCalculationAsync(request.EventId, analysis.Operation, cancellationToken);
            }

            if (analysis.Intent == AiQueryIntent.Semantic)
            {
                return await HandleSemanticCalculationAsync(request.EventId, request.Question, cancellationToken);
            }

            return await HandleGeneralQuestionAsync(request.Question, cancellationToken);
        }

        private async Task<string> HandleStructuredCalculationAsync(Guid eventId, AiQueryOperation operation, CancellationToken cancellationToken)
        {
            return operation switch
            {
                AiQueryOperation.TopPayer => await GetTopPayerAnswerAsync(eventId, cancellationToken),

                AiQueryOperation.EventTotal => await GetEventTotalAnswerAsync(eventId, cancellationToken),

                AiQueryOperation.PerPersonTotals => await GetPerPersonTotalsAnswerAsync(eventId, cancellationToken),

                _ => "I could not determine which calculation to perform."
            };
        }

        private async Task<string> GetTopPayerAnswerAsync(Guid eventId, CancellationToken cancellationToken)
        {
            var memberSepnding = await _expenseRepository.GetMemberSpendings(eventId, cancellationToken);

            if (memberSepnding.Count == 0)
                return "No expenses found for this event.";

            var highestAmount = memberSepnding.Max(x => x.TotalPaid);

            var topPayers = memberSepnding.Where(member => member.TotalPaid == highestAmount).ToList();

            if (topPayers.Count == 1)
            {
                var topPayer = topPayers[0];
                return $"The top payer is {topPayer.Name} with a total of {topPayer.TotalPaid:N2}.";
            }

            var payerNames = string.Join(", ", topPayers.Select(p => p.Name));

            return $"{payerNames} are tied for the highest payment: " + $"{highestAmount:N2} each.";
        }

        private async Task<string> GetEventTotalAnswerAsync(Guid eventId, CancellationToken cancellationToken)
        {
            var totalExpenses = await _expenseRepository.GetEventTotalAsync(eventId, cancellationToken);
            if (totalExpenses is null)
                return "No expenses found for this event.";

            return $"The total expenses for the event is {totalExpenses.Value:N2}.";
        }

        private async Task<string> GetPerPersonTotalsAnswerAsync(Guid eventId, CancellationToken cancellationToken)
        {
            var memberSpendings = await _expenseRepository.GetMemberSpendings(eventId, cancellationToken);
            if (memberSpendings.Count == 0)
                return "No expenses found for this event.";
            
            var answer = new StringBuilder();

            answer.AppendLine("Payments by person:");

            foreach (var member in memberSpendings)
            {
                answer.AppendLine($"{member.Name}: {member.TotalPaid:N2}");
            }
            return answer.ToString().TrimEnd();
        }

        private async Task<string> HandleSemanticCalculationAsync(Guid eventId, string question, CancellationToken cancellationToken)
        {
            var expenses = await GetRelevantExpensesAsync(eventId, question, cancellationToken);

            if (expenses.Count == 0)
            {
                return "I couldn't find any expenses clearly related to that question.";
            }

            var total = expenses.Sum(expense => expense.Amount);

            var answer = new StringBuilder();

            answer.AppendLine($"The matching expenses total {total:N2}:");
            answer.AppendLine();

            foreach (var expense in expenses.OrderByDescending(x => x.Amount))
            {
                answer.AppendLine(
                    $"- {expense.Description}: {expense.Amount:N2}");
            }

            return answer.ToString().TrimEnd();
        }

        private async Task<List<EventExpensesResponnse>>GetRelevantExpensesAsync(Guid eventId, string question, CancellationToken cancellationToken)
        {
            var candidates = await _expenseSearchService.SearchExpensesAsync(eventId, question, cancellationToken);

            if (candidates.Count == 0)
            {
                return new List<EventExpensesResponnse>();
            }

            var relevantExpenseIds = await _expenseRelevanceService.GetRelevantExpenseIdsAsync(question, candidates, cancellationToken);

            if (relevantExpenseIds.Count == 0)
            {
                return new List<EventExpensesResponnse>();
            }

            var candidateIds = candidates.Select(candidate => candidate.ExpenseId).ToHashSet();

            var validExpenseIds = relevantExpenseIds.Where(candidateIds.Contains).Distinct().ToList();

            if (validExpenseIds.Count == 0)
            {
                return new List<EventExpensesResponnse>();
            }

            return await _expenseRepository.GetExpensesByIdsAsync(eventId, validExpenseIds, cancellationToken);
        }

        private async Task<string> HandleSemanticQuestionAsync(Guid eventId, string question, CancellationToken cancellationToken)
        {
            var expenses = await GetRelevantExpensesAsync(eventId, question, cancellationToken);

            if (expenses.Count == 0)
            {
                return "I couldn't find any expenses clearly related to that question.";
            }

            var answer = new StringBuilder();

            answer.AppendLine("I found these matching expenses:");
            answer.AppendLine();

            foreach (var expense in expenses.OrderByDescending(x => x.Amount))
            {
                answer.AppendLine(
                    $"- {expense.Description}: {expense.Amount:N2}");
            }

            return answer.ToString().TrimEnd();
        }

        private async Task<string> HandleGeneralQuestionAsync(string question, CancellationToken cancellationToken)
        {
            var prompt =
                $"""
                You are Circlo AI, an assistant for a group expense
                management application.

                Respond briefly and helpfully.

                Do not invent events, expenses, members, balances,
                payments or financial information.

                User question:
                {question}
                """;

            return await _chatService.AskAsync(prompt, cancellationToken);
        }
    }
}
