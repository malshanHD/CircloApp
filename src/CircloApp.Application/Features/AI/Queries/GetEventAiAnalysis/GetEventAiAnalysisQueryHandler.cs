using CircloApp.Application.Features.Expenses.DTOs;
using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using FluentValidation.Validators;
using MediatR;
using System.Security.Cryptography;
using System.Text;

namespace CircloApp.Application.Features.AI.Queries.GetEventAiAnalysis
{
    public class GetEventAiAnalysisQueryHandler : IRequestHandler<GetEventAiAnalysisQuery, ExpenseAnalysisResponse>
    {
        private readonly IExpensesService _expensesService;
        private readonly IAiExpenseCategorizationService _aiExpenseCategorizationService;
        private readonly IAiService _aiService;
        private readonly IEventAiAnalysisRepository _eventAiAnalysisRepository;

        public GetEventAiAnalysisQueryHandler(IExpensesService expensesService, IAiExpenseCategorizationService aiExpenseCategorization, IAiService aiService, IEventAiAnalysisRepository eventAiAnalysis)
        {
            _expensesService = expensesService;
            _aiExpenseCategorizationService = aiExpenseCategorization;
            _aiService = aiService;
            _eventAiAnalysisRepository = eventAiAnalysis;
        }

        public async Task<ExpenseAnalysisResponse> Handle(GetEventAiAnalysisQuery request, CancellationToken cancellationToken)
        {
            var expenses = await _expensesService.GetEventExpenses(request.EventId, cancellationToken);
            if ( expenses.Count == 0 )
            {
                return new ExpenseAnalysisResponse();
            }

            var existingCategories = await _aiExpenseCategorizationService.GetByEventIdAsync(request.EventId, cancellationToken);

            var existingCategoryIds = existingCategories.Select(x => x.ExpenseId).ToHashSet();

            var uncategorizedExpenses = expenses.Where(x => !existingCategoryIds.Contains(x.Id)).ToList();

            if (uncategorizedExpenses.Count > 0)
            {
                var aiResult = await _aiService.CategorizedExpensesAsync(uncategorizedExpenses, cancellationToken);

                var newCategories = aiResult.Expenses.Select(x => new ExpenseAiCategory
                {
                    Id = Guid.NewGuid(),
                    ExpenseId = x.ExpenseId,
                    Category = x.Category,
                    Model = "gpt-5mini",
                    CreatedAt = DateTime.UtcNow,
                }).ToList();

                await _aiExpenseCategorizationService.AddRangeAsync(newCategories, cancellationToken);
                existingCategories.AddRange(newCategories);
            }

            var totalExpenses = expenses.Sum(x => x.Amount);

            var categorizedExpenses = existingCategories.Join(expenses, 
                                                                category => category.ExpenseId,
                                                                expenses => expenses.Id,
                                                                (category, expense) => new
                                                                {
                                                                    category.Category,
                                                                    expense.Amount
                                                                }).ToList();

            var categorySummaries = categorizedExpenses.GroupBy(x => x.Category)
                                                       .Select(group =>
                                                       {
                                                           var categoryTotal = group.Sum(x => x.Amount);

                                                           return new CategorySummaryDto
                                                           {
                                                               Category = group.Key,
                                                               Amount = categoryTotal,
                                                               Percentage = totalExpenses > 0 ? Math.Round(categoryTotal / totalExpenses * 100, 2) : 0
                                                           };
                                                       }).OrderByDescending(x => x.Amount).ToList();

            var currentHash = CreateExpenseDataHash(expenses);

            var cachedAnalysis = await _eventAiAnalysisRepository.GetEventAiAnalysisAsync(request.EventId, cancellationToken);

            string summary;

            if (cachedAnalysis is not null && cachedAnalysis.DataHash == currentHash)
            {
                summary = cachedAnalysis.Summary;
            }
            else
            {
                summary = await _aiService.GenerateExpenseSummaryAsync(totalExpenses, categorySummaries, cancellationToken);

                var analysis = new EventAiAnalysis
                {
                    Id = cachedAnalysis?.Id ?? Guid.NewGuid(),
                    EventId = request.EventId,
                    Summary = summary,
                    DataHash = currentHash,
                    Model = "gpt-5-mini",
                    UpdatedAt = DateTime.UtcNow,
                };

                await _eventAiAnalysisRepository.SaveAsync(analysis, cancellationToken);
            }

            return new ExpenseAnalysisResponse
            {
                TotalExpense = totalExpenses,
                Categories = categorySummaries,
                Summary = summary,
            };
        }

        private static string CreateExpenseDataHash(IEnumerable<EventExpensesResponnse> expenses)
        {
            var data = string.Join("|", expenses.OrderBy(x => x.Id).Select(x => $"{x.Id}:{x.Description}:{x.Amount}"));

            var bytes = Encoding.UTF8.GetBytes(data);

            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
        }
    }
}
