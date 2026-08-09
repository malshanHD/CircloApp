using CircloApp.Application.Features.Expenses.DTOs;
using CircloApp.Application.Interfaces;

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

            var totalCost = expensesSummary.Sum(e => e.Amount);

            var totalMembers = await _eventMemberRepository.GetEventParticipantCountAsync(eventId, cancellationToken);

            var personShouldPay = Math.Round(totalCost / totalMembers, 2);

            var userBalances = expensesSummary
                .GroupBy(e => e.PaidUser)
                .Select(g =>
                {
                    var totalPaid = g.Sum(e => e.Amount);
                    var balance = totalPaid - personShouldPay;

                    return new UserBalanceDto
                    {
                        UserName = g.Key,
                        TotalPaid = totalPaid,
                        Balance = balance,
                        Status = balance switch
                        {
                            > 0 => $"Is owed {balance:C2}",
                            < 0 => $"Owes {Math.Abs(balance):C2}",
                            _ => "Settled"
                        }
                    };
                }).ToList();

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
