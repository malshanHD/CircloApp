using CircloApp.Application.Exceptions;
using CircloApp.Application.Features.Expenses.DTOs;
using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
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

            var expenses = new Expense
            {
                Id = Guid.NewGuid(),
                EventId = request.EventId,
                PaidByUserId = _currentUserService.UserId,
                Amount = request.CreateExpenses.Amount,
                Description = request.CreateExpenses.Description,
                ExpenseDate = _dateTimeProvider.UtcNow,
                CreatedAt = _dateTimeProvider.UtcNow,
                UpdatedAt = _dateTimeProvider.UtcNow,
                IsDeleted = false
            };

            await _service.AddExpense(expenses, cancellationToken);
            await _unitWork.SaveChangesAsync(cancellationToken);

            return await _eventExpenseSummaryHelper.EventExpensesSummaryAsync(request.EventId, cancellationToken);
        }
    }
}
