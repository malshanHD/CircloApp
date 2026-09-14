using CircloApp.Application.Interfaces;
using CircloApp.Application.Interfaces.AI;

namespace CircloApp.Infrastructure.AI
{
    public class ExpenseCalculationService : IExpenseCalculationService
    {
        private readonly IExpensesService _expensesService;

        public ExpenseCalculationService(IExpensesService expensesService)
        {
            _expensesService = expensesService;
        }

        public async Task<decimal> CalculateTotalAsync(Guid eventId, string question, CancellationToken cancellationToken)
        {
            var expenses = await _expensesService.GetEventExpenses(eventId, cancellationToken);
            var releventExpenses = expenses.Where(x => question.Contains(x.Description, StringComparison.OrdinalIgnoreCase)).ToList();
            return releventExpenses.Sum(x => x.Amount);
        }
    }
}
