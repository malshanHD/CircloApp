using CircloApp.Application.Exceptions;
using CircloApp.Application.Features.PersonalExpenses.Commands.AddExpenses;
using CircloApp.Application.Features.PersonalExpenses.DTOs;
using CircloApp.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace CircloApp.Application.Features.PersonalExpenses;

public record ListPersonalExpenses(Guid UserId, ExpenseFilter Filter) : IRequest<ExpensePage>;
public record GetPersonalExpense(Guid UserId, Guid Id) : IRequest<ExpenseItem>;
public record UpdatePersonalExpense(Guid UserId, Guid Id, CreatePersonalExpenseRequest Data) : IRequest<ExpenseItem>;
public record DeletePersonalExpense(Guid UserId, Guid Id) : IRequest<Unit>;

public class ExpenseRequestValidator : AbstractValidator<CreatePersonalExpenseRequest>
{
    public ExpenseRequestValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Amount).GreaterThan(0).LessThanOrEqualTo(9999999999999999.99m).PrecisionScale(18, 2, false);
        RuleFor(x => x.ExpenseDate).NotEmpty();
        RuleFor(x => x.PaymentMethod).MaximumLength(50);
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
public class AddPersonalExpenseValidator : AbstractValidator<AddExpenseCommand>
{
    public AddPersonalExpenseValidator() => RuleFor(x => x.CreatePersonalExpenseRequest).NotNull().SetValidator(new ExpenseRequestValidator());
}
public class UpdatePersonalExpenseValidator : AbstractValidator<UpdatePersonalExpense>
{
    public UpdatePersonalExpenseValidator() => RuleFor(x => x.Data).NotNull().SetValidator(new ExpenseRequestValidator());
}
public class ExpenseCrudHandler(IPersonalExpenseRepository expenses,
    IPersonalExpenseCategoryRepository categories, IUnitOfWork unitOfWork) :
    IRequestHandler<ListPersonalExpenses, ExpensePage>, IRequestHandler<GetPersonalExpense, ExpenseItem>,
    IRequestHandler<UpdatePersonalExpense, ExpenseItem>, IRequestHandler<DeletePersonalExpense, Unit>
{
    public async Task<ExpensePage> Handle(ListPersonalExpenses q, CancellationToken ct)
    {
        var (from, to) = q.Filter.ValidateAndGetRange();
        var result = await expenses.ListAsync(q.UserId, q.Filter, from, to, ct);
        return new(result.Items.Select(ExpenseItem.From).ToList(), q.Filter.Page, q.Filter.PageSize,
            result.Count, (int)Math.Ceiling(result.Count / (double)q.Filter.PageSize));
    }
    public async Task<ExpenseItem> Handle(GetPersonalExpense q, CancellationToken ct) =>
        ExpenseItem.From(await Find(q.UserId, q.Id, false, ct));
    public async Task<ExpenseItem> Handle(UpdatePersonalExpense q, CancellationToken ct)
    {
        var e = await Find(q.UserId, q.Id, true, ct);
        var categoryId = q.Data.CategoryId ?? await categories.GetUncategorizedIdAsync(ct);
        if (!categoryId.HasValue || !await categories.IsAvailableForUserAsync(categoryId.Value, q.UserId, ct))
            throw new BadRequestException("The selected expense category is invalid.");
        e.Description = q.Data.Description.Trim(); e.Amount = q.Data.Amount;
        e.ExpenseDate = q.Data.ExpenseDate; e.CategoryId = categoryId;
        e.PaymentMethod = string.IsNullOrWhiteSpace(q.Data.PaymentMethod) ? null : q.Data.PaymentMethod.Trim();
        e.Note = string.IsNullOrWhiteSpace(q.Data.Note) ? null : q.Data.Note.Trim();
        await unitOfWork.SaveChangesAsync(ct);
        return ExpenseItem.From(await Find(q.UserId, q.Id, false, ct));
    }
    public async Task<Unit> Handle(DeletePersonalExpense q, CancellationToken ct)
    {
        var e = await Find(q.UserId, q.Id, true, ct);
        e.IsDeleted = true;
        await unitOfWork.SaveChangesAsync(ct);
        return Unit.Value;
    }
    private async Task<CircloApp.Domain.Entities.PersonalExpense> Find(Guid user, Guid id, bool tracking, CancellationToken ct) =>
        await expenses.GetAsync(user, id, tracking, ct) ?? throw new NotFoundException("Personal expense not found.");
}
