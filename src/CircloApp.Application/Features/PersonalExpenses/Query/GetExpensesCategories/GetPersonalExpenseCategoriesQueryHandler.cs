using CircloApp.Application.Features.PersonalExpenses.DTOs;
using CircloApp.Application.Interfaces;
using MediatR;

namespace CircloApp.Application.Features.PersonalExpenses.Query.GetExpensesCategories
{
    public class GetPersonalExpenseCategoriesQueryHandler : IRequestHandler<GetPersonalExpenseCategoriesQuery, IReadOnlyList<PersonalExpenseCategoryResponse>>
    {
        private readonly IPersonalExpenseCategoryRepository _personalExpenseCategoryRepository;
        public GetPersonalExpenseCategoriesQueryHandler(IPersonalExpenseCategoryRepository personalExpense)
        {
            _personalExpenseCategoryRepository = personalExpense;
        }
        public async Task<IReadOnlyList<PersonalExpenseCategoryResponse>> Handle(GetPersonalExpenseCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _personalExpenseCategoryRepository.GetAvailableForUserAsync(request.UserId, cancellationToken);

            return categories.Select(category => new PersonalExpenseCategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Icon = category.Icon,
                Color = category.Color,
                IsSystem = category.IsSystem,
            }).ToList();
        }
    }
}
