using CircloApp.Application.Features.Expenses.DTOs;
using MediatR;

namespace CircloApp.Application.Features.Expenses.Queries.GetUserExpenses
{
    public record GetUserExpenses : IRequest<List<GetUserAllExpensesResponse>>;
}
