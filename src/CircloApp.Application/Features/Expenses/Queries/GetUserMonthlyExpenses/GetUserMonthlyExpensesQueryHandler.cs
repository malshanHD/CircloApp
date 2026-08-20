using CircloApp.Application.Features.Expenses.DTOs;
using CircloApp.Application.Interfaces;
using MediatR;

namespace CircloApp.Application.Features.Expenses.Queries.GetUserMonthlyExpenses
{
    public class GetUserMonthlyExpensesQueryHandler : IRequestHandler<GetUserMonthlyExpensesQuery, List<GetUserMonthlyExpensesResponse>>
    {
        private readonly IExpensesService _expensesService;
        private readonly ICurrentUserService _currentUserService;

        public GetUserMonthlyExpensesQueryHandler(IExpensesService expensesService, ICurrentUserService currentUserService)
        {
            _expensesService = expensesService;
            _currentUserService = currentUserService;
        }

        public async Task<List<GetUserMonthlyExpensesResponse>> Handle(GetUserMonthlyExpensesQuery request, CancellationToken cancellationToken)
        {
            var result = await _expensesService.GetUserExpensesByMonth(_currentUserService.UserId, request.year, cancellationToken);
            return result ?? [];
        }
    }
}

