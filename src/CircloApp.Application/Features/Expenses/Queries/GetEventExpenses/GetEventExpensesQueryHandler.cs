using CircloApp.Application.Exceptions;
using CircloApp.Application.Features.Expenses.DTOs;
using CircloApp.Application.Interfaces;
using MediatR;

namespace CircloApp.Application.Features.Expenses.Queries.GetEventExpenses
{
    public class GetEventExpensesQueryHandler : IRequestHandler<GetEventExpensesQuery, EventExpensesSummaryResponse>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IExpensesService _expensesService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IEventMemberRepository _eventMemberRepository;
        private readonly IEventExpenseSummaryHelper _eventExpenseSummaryHelper;

        public GetEventExpensesQueryHandler(ICurrentUserService currentUserService, IExpensesService expensesService, IDateTimeProvider dateTimeProvider, 
                                            IEventMemberRepository eventMemberRepository, IEventExpenseSummaryHelper eventExpense)
        {
            _currentUserService = currentUserService;
            _expensesService = expensesService;
            _dateTimeProvider = dateTimeProvider;
            _eventMemberRepository = eventMemberRepository;
            _eventExpenseSummaryHelper = eventExpense;
        }

        public async Task<EventExpensesSummaryResponse> Handle(GetEventExpensesQuery request, CancellationToken cancellationToken)
        {
            var eventMember = await _eventMemberRepository.IsMemberExist(request.EventId, _currentUserService.UserId, cancellationToken);
            if (!eventMember)
                throw new BadRequestException("Event not found");

            return await _eventExpenseSummaryHelper.EventExpensesSummaryAsync(request.EventId, cancellationToken);
        }
    }
}
