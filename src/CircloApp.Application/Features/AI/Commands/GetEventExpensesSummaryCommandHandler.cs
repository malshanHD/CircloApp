using CircloApp.Application.Features.AI.DTO;
using CircloApp.Application.Interfaces;
using MediatR;

namespace CircloApp.Application.Features.AI.Commands
{
    public class GetEventExpensesSummaryCommandHandler : IRequestHandler<GetEventExpensesSummaryCommand, CategorizedExpensesResponse>
    {
        private readonly IExpensesService _service;
        private readonly IAiService _aiService;

        public GetEventExpensesSummaryCommandHandler(IExpensesService expensesService, IAiService aiService)
        {
            _service = expensesService;
            _aiService = aiService;
        }

        public async Task<CategorizedExpensesResponse> Handle(GetEventExpensesSummaryCommand request, CancellationToken cancellationToken)
        {
            var expenses = await _service.GetEventExpenses(request.EventId, cancellationToken);
            var simplifiedExpenses = expenses.Select(e => new
            {
                e.Description,
                e.Amount
            });

            return await _aiService.CategorizedExpensesAsync(expenses, cancellationToken);
        }
    }
}
