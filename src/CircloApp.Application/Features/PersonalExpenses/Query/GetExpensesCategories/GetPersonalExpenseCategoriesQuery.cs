using CircloApp.Application.Features.PersonalExpenses.DTOs;
using MediatR;

namespace CircloApp.Application.Features.PersonalExpenses.Query.GetExpensesCategories
{
    public record GetPersonalExpenseCategoriesQuery(Guid UserId) : IRequest<IReadOnlyList<PersonalExpenseCategoryResponse>>;
}
