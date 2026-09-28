using CircloApp.Application.Features.PersonalExpenses.DTOs;
using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using FluentValidation;
using MediatR;

namespace CircloApp.Application.Features.PersonalExpenses;

public record SettingsRequest(decimal DefaultMonthlyLimit, string CurrencyCode = "LKR", decimal WarningPercentage = 80);
public record SettingsResponse(decimal? DefaultMonthlyLimit, string CurrencyCode, decimal WarningPercentage);
public record BudgetRequest(decimal LimitAmount);
public record BudgetResponse(int Year, int Month, decimal? LimitAmount, decimal? EffectiveMonthlyLimit, string LimitSource);
public record GetPersonalSettings(Guid UserId) : IRequest<SettingsResponse>;
public record SavePersonalSettings(Guid UserId, SettingsRequest Data) : IRequest<SettingsResponse>;
public record GetPersonalBudget(Guid UserId, int Year, int Month) : IRequest<BudgetResponse>;
public record SavePersonalBudget(Guid UserId, int Year, int Month, BudgetRequest Data) : IRequest<BudgetResponse>;
public record RemovePersonalBudget(Guid UserId, int Year, int Month) : IRequest<Unit>;
public class SettingsValidator : AbstractValidator<SavePersonalSettings>
{
    public SettingsValidator()
    {
        RuleFor(x => x.Data.DefaultMonthlyLimit).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, false);
        RuleFor(x => x.Data.CurrencyCode).NotEmpty().Must(x => x != null && System.Text.RegularExpressions.Regex.IsMatch(x.Trim(), "^[A-Za-z]{3}$"))
            .WithMessage("Use a three-letter currency code, for example LKR.");
        RuleFor(x => x.Data.WarningPercentage).InclusiveBetween(1, 100).PrecisionScale(5, 2, false);
    }
}
public class BudgetValidator : AbstractValidator<SavePersonalBudget>
{
    public BudgetValidator() => RuleFor(x => x.Data.LimitAmount).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, false);
}
public class PersonalBudgetHandler(IPersonalBudgetRepository repository, IUnitOfWork unitOfWork) :
    IRequestHandler<GetPersonalSettings, SettingsResponse>, IRequestHandler<SavePersonalSettings, SettingsResponse>,
    IRequestHandler<GetPersonalBudget, BudgetResponse>, IRequestHandler<SavePersonalBudget, BudgetResponse>, IRequestHandler<RemovePersonalBudget, Unit>
{
    public async Task<SettingsResponse> Handle(GetPersonalSettings q, CancellationToken ct)
    {
        var settings = await repository.SettingsAsync(q.UserId, false, ct);
        return new(settings?.DefaultMonthlyLimit, settings?.CurrencyCode ?? "LKR", settings?.WarningPercentage ?? 80);
    }
    public async Task<SettingsResponse> Handle(SavePersonalSettings q, CancellationToken ct)
    {
        var settings = await repository.SettingsAsync(q.UserId, true, ct);
        if (settings == null)
        {
            settings = new PersonalExpenseSettings { UserId = q.UserId };
            await repository.AddSettingsAsync(settings, ct);
        }
        settings.IsDeleted = false;
        settings.DefaultMonthlyLimit = q.Data.DefaultMonthlyLimit;
        settings.CurrencyCode = q.Data.CurrencyCode.Trim().ToUpperInvariant();
        settings.WarningPercentage = q.Data.WarningPercentage;
        await unitOfWork.SaveChangesAsync(ct);
        return new(settings.DefaultMonthlyLimit, settings.CurrencyCode, settings.WarningPercentage);
    }
    public async Task<BudgetResponse> Handle(GetPersonalBudget q, CancellationToken ct)
    {
        PersonalExpenseRules.ValidateMonth(q.Year, q.Month);
        var budget = await repository.BudgetAsync(q.UserId, q.Year, q.Month, false, ct);
        var settings = await repository.SettingsAsync(q.UserId, false, ct);
        return new(q.Year, q.Month, budget?.LimitAmount, budget?.LimitAmount ?? settings?.DefaultMonthlyLimit,
            budget != null ? "MonthlyOverride" : settings != null ? "Default" : "NotConfigured");
    }
    public async Task<BudgetResponse> Handle(SavePersonalBudget q, CancellationToken ct)
    {
        PersonalExpenseRules.ValidateMonth(q.Year, q.Month);
        var budget = await repository.BudgetAsync(q.UserId, q.Year, q.Month, true, ct);
        if (budget == null)
        {
            budget = new MonthlyExpenseBudget { UserId = q.UserId, Year = q.Year, Month = q.Month };
            await repository.AddBudgetAsync(budget, ct);
        }
        budget.IsDeleted = false;
        budget.LimitAmount = q.Data.LimitAmount;
        await unitOfWork.SaveChangesAsync(ct);
        return new(q.Year, q.Month, budget.LimitAmount, budget.LimitAmount, "MonthlyOverride");
    }
    public async Task<Unit> Handle(RemovePersonalBudget q, CancellationToken ct)
    {
        PersonalExpenseRules.ValidateMonth(q.Year, q.Month);
        var budget = await repository.BudgetAsync(q.UserId, q.Year, q.Month, true, ct);
        if (budget != null) { budget.IsDeleted = true; await unitOfWork.SaveChangesAsync(ct); }
        return Unit.Value;
    }
}
