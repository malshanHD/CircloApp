using CircloApp.Application.Exceptions;
using CircloApp.Application.Features.Expenses.DTOs;
using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using CircloApp.Domain.Enums;
using MediatR;

namespace CircloApp.Application.Features.Expenses.Commands.AddExpenses
{
    public class AddExpensesCommandHandler : IRequestHandler<AddExpensesCommand, EventExpensesSummaryResponse>
    {
        private readonly IExpensesService _service;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IUnitOfWork _unitWork;
        private readonly IEventMemberRepository _eventMemberRepository;
        private readonly IEventExpenseSummaryHelper _eventExpenseSummaryHelper;

        public AddExpensesCommandHandler(IExpensesService expensesService, ICurrentUserService currentUserService, 
                                         IDateTimeProvider dateTimeProvider, IUnitOfWork unitOfWork, IEventMemberRepository eventMember, IEventExpenseSummaryHelper eventExpenseSummary)
        {
            _service = expensesService;
            _currentUserService = currentUserService;
            _dateTimeProvider = dateTimeProvider;
            _unitWork = unitOfWork;
            _eventMemberRepository = eventMember;
            _eventExpenseSummaryHelper = eventExpenseSummary;
        }

        public async Task<EventExpensesSummaryResponse> Handle(AddExpensesCommand request, CancellationToken cancellationToken)
        {
            var eventMember = await _eventMemberRepository.IsMemberExist(request.EventId, _currentUserService.UserId, cancellationToken);
            if (!eventMember)
                throw new BadRequestException("Event not found");

            if (request.CreateExpenses.TransactionType == TransactionType.Settlement)
            {
                await AutoSettleUpAsync(request.EventId, _currentUserService.UserId, request.CreateExpenses.Amount, cancellationToken);
            }

            else
            {
                var expenses = new Expense
                {
                    Id = Guid.NewGuid(),
                    EventId = request.EventId,
                    PaidByUserId = _currentUserService.UserId,
                    Amount = request.CreateExpenses.Amount,
                    Description = request.CreateExpenses.Description,
                    Type = request.CreateExpenses.TransactionType,
                    ExpenseDate = _dateTimeProvider.UtcNow,
                    CreatedAt = _dateTimeProvider.UtcNow,
                    UpdatedAt = _dateTimeProvider.UtcNow,
                    IsDeleted = false
                };

                await _service.AddExpense(expenses, cancellationToken);
                await _unitWork.SaveChangesAsync(cancellationToken);
            }

            return await _eventExpenseSummaryHelper.EventExpensesSummaryAsync(request.EventId, cancellationToken);
        }

        public async Task AutoSettleUpAsync(Guid eventId, Guid payerUserId, decimal settlementAmount, CancellationToken cancellationToken)
        {
            var transactions = await _service.GetEventExpenses(eventId, cancellationToken);
            var expenses = transactions.Where(e => e.Type == TransactionType.Expense).ToList();
            var settlements = transactions.Where(e => e.Type == TransactionType.Settlement).ToList();

            var totalAmount = expenses.Sum(e => e.Amount);
            var eventMembers = await _eventMemberRepository.GetEventMembers(eventId, cancellationToken);
            var totalMembers = eventMembers.Count();

            var personShouldPay = totalMembers > 0
                ? Math.Round(totalAmount / totalMembers, 2)
                : 0m;

            // 1. Group expenses paid
            var paidExpensesByUser = expenses
                .GroupBy(e => e.PaidUserId)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

            // 2. Group settlements paid (increases payer's balance)
            var settlementsPaidByUser = settlements
                .GroupBy(e => e.PaidUserId)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

            // 3. Group settlements received (decreases recipient's balance)
            var settlementsReceivedByUser = settlements
                .GroupBy(e => e.PaidToUserId)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

            // 4. Calculate true Net Balances
            var memberBalances = eventMembers
                .Select(member =>
                {
                    var userId = member.UserId;

                    var expensesPaid = paidExpensesByUser.GetValueOrDefault(userId, 0m);
                    var settlementsPaid = settlementsPaidByUser.GetValueOrDefault(userId, 0m);
                    var settlementsReceived = settlementsReceivedByUser.GetValueOrDefault(userId, 0m);

                    // Net Contribution = Expenses Paid + Settlements Paid Out - Settlements Received
                    var netContribution = expensesPaid + settlementsPaid - settlementsReceived;
                    var balance = Math.Round(netContribution - personShouldPay, 2);

                    return new
                    {
                        UserId = userId,
                        MemberName = member.User.FirstName,
                        PaidAmount = expensesPaid,
                        Balance = balance
                    };
                })
                .ToList();

            var currentUserBalance = memberBalances.FirstOrDefault(x => x.UserId == payerUserId);

            if (currentUserBalance == null)
            {
                throw new BadRequestException("User is not a member of this event.");
            }

            // Creditors sorted by who is owed the most (Balance is already remaining balance!)
            var creditors = memberBalances
                .Where(x => x.UserId != payerUserId && x.Balance > 0)
                .OrderByDescending(x => x.Balance)
                .ToList();

            var userBalance = currentUserBalance.Balance;
            var expensesData = new List<Expense>();

            if (userBalance < 0)
            {
                foreach (var creditor in creditors)
                {
                    if (settlementAmount <= 0)
                        break;

                    // creditor.Balance is now accurate! No need to query 'alreadySettledAmounts' here.
                    var paymentAmount = Math.Min(settlementAmount, creditor.Balance);

                    expensesData.Add(new Expense
                    {
                        Id = Guid.NewGuid(),
                        EventId = eventId,
                        PaidByUserId = payerUserId,
                        PaidToUserId = creditor.UserId,
                        Amount = paymentAmount,
                        Description = $"Settlement payment to {creditor.MemberName}",
                        Type = TransactionType.Settlement,
                        ExpenseDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    });

                    settlementAmount -= paymentAmount;
                }

                await _service.AddRangeAsync(expensesData, cancellationToken);
                await _unitWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
