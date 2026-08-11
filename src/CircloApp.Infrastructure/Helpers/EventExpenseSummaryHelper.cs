using CircloApp.Application.Exceptions;
using CircloApp.Application.Features.Expenses.DTOs;
using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using CircloApp.Domain.Enums;
using CircloApp.Infrastructure.Authentication;
using CircloApp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CircloApp.Infrastructure.Helpers
{
    public class EventExpenseSummaryHelper : IEventExpenseSummaryHelper
    {
        private readonly IExpensesService _service;
        private readonly IEventMemberRepository _eventMemberRepository;

        public EventExpenseSummaryHelper(IExpensesService expensesService, IEventMemberRepository eventMember)
        {
            _service = expensesService;
            _eventMemberRepository = eventMember;
        }
        public async Task<EventExpensesSummaryResponse> EventExpensesSummaryAsync(Guid eventId, CancellationToken cancellationToken)
        {
            var expensesSummary = await _service.GetEventExpenses(eventId, cancellationToken);

            // 1. Calculate total costs (Expenses only)
            var totalCost = expensesSummary
                .Where(e => e.Type == TransactionType.Expense)
                .Sum(e => e.Amount);

            var eventMembers = await _eventMemberRepository.GetEventMembers(eventId, cancellationToken);
            var totalMembers = eventMembers.Count();

            // 2. Calculate fair share per person
            var personShouldPay = totalMembers > 0
                ? Math.Round(totalCost / totalMembers, 2)
                : 0m;

            // 3. Group standard expenses paid by each user
            var paidExpensesByUser = expensesSummary
                .Where(e => e.Type == TransactionType.Expense)
                .GroupBy(e => e.PaidUserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(e => e.Amount));

            // 4a. Group settlements PAID BY each user (Increases their contribution)
            var settlementsPaidByUser = expensesSummary
                .Where(e => e.Type == TransactionType.Settlement)
                .GroupBy(e => e.PaidUserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(e => e.Amount));

            // 4b. Group settlements RECEIVED BY each user (Deducted from what they are owed)
            var settlementsReceivedByUser = expensesSummary
                .Where(e => e.Type == TransactionType.Settlement && e.PaidToUserId != Guid.Empty)
                .GroupBy(e => e.PaidToUserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(e => e.Amount));

            // 5. Build per-user summary
            var userBalances = eventMembers
                .Select(member =>
                {
                    var userId = member.User.Id;

                    var totalExpensesPaid = paidExpensesByUser.GetValueOrDefault(userId, 0m);
                    var settlementsPaid = settlementsPaidByUser.GetValueOrDefault(userId, 0m);
                    var settlementsReceived = settlementsReceivedByUser.GetValueOrDefault(userId, 0m);

                    // Formula:
                    // Net Balance = (Expenses Paid + Settlements Paid - Settlements Received) - Fair Share
                    var balance = (totalExpensesPaid + settlementsPaid - settlementsReceived) - personShouldPay;

                    return new UserBalanceDto
                    {
                        UserId = member.UserId,
                        UserName = member.User.Username,
                        TotalPaid = totalExpensesPaid,
                        TotalSettled = settlementsPaid,
                        TotalSettledReceived = settlementsReceived,
                        Balance = balance,
                        Status = balance switch
                        {
                            > 0 => $"Is owed {balance:C2}",
                            < 0 => $"Owes {Math.Abs(balance):C2}",
                            _ => "Settled"
                        }
                    };
                })
                .ToList();

            return new EventExpensesSummaryResponse
            {
                EventId = eventId,
                TotalCost = totalCost,
                EqualSharePerPerson = personShouldPay,
                UserBalances = userBalances
            };
        }
    }
}
