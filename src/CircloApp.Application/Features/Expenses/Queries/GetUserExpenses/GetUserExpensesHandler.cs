using CircloApp.Application.Features.Expenses.DTOs;
using CircloApp.Application.Interfaces;
using MediatR;

namespace CircloApp.Application.Features.Expenses.Queries.GetUserExpenses
{
    public class GetUserExpensesHandler : IRequestHandler<GetUserExpenses, List<GetUserAllExpensesResponse>>
    {
        private readonly IExpensesService _service;
        private readonly ICurrentUserService _currentUserService;

        public GetUserExpensesHandler(IExpensesService expensesService, ICurrentUserService currentUserService)
        {
            _service = expensesService;
            _currentUserService = currentUserService;
        }

        public async Task<List<GetUserAllExpensesResponse>> Handle(GetUserExpenses request, CancellationToken cancellationToken)
        {
            var result = await _service.GetUserExpensesByEventAsync(_currentUserService.UserId, cancellationToken);
            return result ?? [];
        }
    }
}
