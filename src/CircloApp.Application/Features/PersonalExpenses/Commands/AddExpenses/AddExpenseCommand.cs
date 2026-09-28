using CircloApp.Application.Features.PersonalExpenses.DTOs;
using MediatR;

namespace CircloApp.Application.Features.PersonalExpenses.Commands.AddExpenses
{
    public record AddExpenseCommand(CreatePersonalExpenseRequest CreatePersonalExpenseRequest, Guid UserId) : IRequest<PersonalExpensesResponse>;
}
