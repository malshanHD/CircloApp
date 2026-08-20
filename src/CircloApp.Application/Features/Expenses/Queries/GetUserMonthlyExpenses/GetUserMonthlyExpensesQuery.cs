using CircloApp.Application.Features.Expenses.DTOs;
using MediatR;

namespace CircloApp.Application.Features.Expenses.Queries.GetUserMonthlyExpenses
{
    public record GetUserMonthlyExpensesQuery(int year) : IRequest<List<GetUserMonthlyExpensesResponse>>;
}
