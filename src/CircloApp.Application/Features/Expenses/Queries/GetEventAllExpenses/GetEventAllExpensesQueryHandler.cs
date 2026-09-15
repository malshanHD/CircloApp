using CircloApp.Application.Features.Expenses.DTOs;
using CircloApp.Application.Interfaces;
using MediatR;

namespace CircloApp.Application.Features.Expenses.Queries.GetEventAllExpenses
{
    public class GetEventAllExpensesQueryHandler : IRequestHandler<GetEventAllExpensesQuery, List<EventAllExpensesResponse>>
    {
        private readonly IExpensesService _expensesService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IEventMemberRepository _eventMemberRepository;

        public GetEventAllExpensesQueryHandler(IExpensesService expensesService, ICurrentUserService currentUserService, IEventMemberRepository eventMemberRepository)
        {
            _expensesService = expensesService;
            _currentUserService = currentUserService;
            _eventMemberRepository = eventMemberRepository;
        }

        public async Task<List<EventAllExpensesResponse>> Handle(GetEventAllExpensesQuery request, CancellationToken cancellationToken)
        {
            var isMemberExist = await _eventMemberRepository.IsMemberExist(request.EventId, _currentUserService.UserId, cancellationToken);
            if (!isMemberExist)
            {
                throw new InvalidOperationException("User is not a member of the specified event.");
            }

            var expenses = await _expensesService.GetEventExpenses(request.EventId, cancellationToken);

            if (expenses == null || !expenses.Any())
            {
                return [];
            }

            var response = expenses.Select(expense => new EventAllExpensesResponse
            {
                PaidUser = expense.PaidUser,
                Description = expense.Description,
                Amount = expense.Amount,
                DateAndTime = expense.DateAndTime,
                Type = expense.Type
            }).ToList();

            return response;
        }
    }
}
