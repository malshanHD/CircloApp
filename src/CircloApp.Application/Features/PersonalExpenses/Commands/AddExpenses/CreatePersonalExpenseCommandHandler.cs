using CircloApp.Application.Exceptions;
using CircloApp.Application.Features.PersonalExpenses.DTOs;
using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using MediatR;

namespace CircloApp.Application.Features.PersonalExpenses.Commands.AddExpenses
{
    public class CreatePersonalExpenseCommandHandler : IRequestHandler<AddExpenseCommand, PersonalExpensesResponse>
    {
        private readonly IPersonalExpenseRepository _personalExpenseRepository;
        private readonly IPersonalExpenseCategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreatePersonalExpenseCommandHandler(
            IPersonalExpenseRepository personalExpenseRepository,
            IPersonalExpenseCategoryRepository categoryRepository,
            IUnitOfWork unitOfWork)
        {
            _personalExpenseRepository = personalExpenseRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PersonalExpensesResponse> Handle(AddExpenseCommand command, CancellationToken cancellationToken)
        {
            var request = command.CreatePersonalExpenseRequest;

            var categoryId = await ResolveCategoryIdAsync(request.CategoryId, command.UserId, cancellationToken);

            var expense = new PersonalExpense
            {
                UserId = command.UserId,
                CategoryId = categoryId,
                Description = request.Description.Trim(),
                Amount = request.Amount,
                ExpenseDate = request.ExpenseDate,
                PaymentMethod = Normalize(request.PaymentMethod),
                Note = Normalize(request.Note)
            };

            await _personalExpenseRepository.AddAsync(
                expense,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new PersonalExpensesResponse
            {
                Id = expense.Id,
                Description = expense.Description,
                Amount = expense.Amount,
                ExpenseDate = expense.ExpenseDate,
                CategoryId = expense.CategoryId,
                PaymentMethod = expense.PaymentMethod,
                Note = expense.Note,
                CreatedAt = expense.CreatedAt
            };
        }

        private async Task<Guid> ResolveCategoryIdAsync(Guid? requestedCategoryId, Guid userId, CancellationToken cancellationToken)
        {
            if (requestedCategoryId.HasValue)
            {
                var categoryAvailable = await _categoryRepository.IsAvailableForUserAsync(requestedCategoryId.Value, userId, cancellationToken);

                if (!categoryAvailable)
                {
                    throw new BadRequestException(
                        "The selected expense category is invalid.");
                }

                return requestedCategoryId.Value;
            }

            var uncategorizedId = await _categoryRepository.GetUncategorizedIdAsync(cancellationToken);

            if (!uncategorizedId.HasValue)
            {
                throw new InvalidOperationException(
                    "The Uncategorized system category is not configured.");
            }

            return uncategorizedId.Value;
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
