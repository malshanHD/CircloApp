using CircloApp.Application.Interfaces;
using MediatR;
using System.Text.Json;

namespace CircloApp.Application.Features.AI.Commands
{
    public class GetEventExpensesSummaryCommandHandler : IRequestHandler<GetEventExpensesSummaryCommand, string>
    {
        private readonly IExpensesService _service;
        private readonly IAiExpenseCategorizationService _aiExpenseCategorizationService;

        public GetEventExpensesSummaryCommandHandler(IExpensesService expensesService, IAiExpenseCategorizationService aiExpenseCategorizationService)
        {
            _service = expensesService;
            _aiExpenseCategorizationService = aiExpenseCategorizationService;
        }

        public async Task<string> Handle(GetEventExpensesSummaryCommand request, CancellationToken cancellationToken)
        {
            var expenses = await _service.GetEventExpenses(request.EventId, cancellationToken);
            var simplifiedExpenses = expenses.Select(e => new
            {
                e.Description,
                e.Amount
            });

            var expensesJson = JsonSerializer.Serialize(simplifiedExpenses);
            return await _aiExpenseCategorizationService.AnalyzeSpendingAsync(expensesJson, cancellationToken);
        }
    }
}
