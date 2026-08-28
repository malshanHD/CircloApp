using CircloApp.Application.Interfaces;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

namespace CircloApp.Infrastructure.AI.Plugins
{
    public class CircloExpensePlugin
    {
        private readonly IExpensesService _expensesService;

        public CircloExpensePlugin(IExpensesService expensesService)
        {
            _expensesService = expensesService;
        }

        [KernelFunction("get_spending_by_member")]
        [Description("Gets how much each member has paid for expenses in a Circlo event.")]
        public async Task<string> GetSpendingByMemberAsync([Description("The unique ID of the Circlo event")] Guid eventId, CancellationToken cancellationToken)
        {
            var spending = await _expensesService.GetMemberSpendings(eventId, cancellationToken);
            return JsonSerializer.Serialize(spending);
        }
    }
}
