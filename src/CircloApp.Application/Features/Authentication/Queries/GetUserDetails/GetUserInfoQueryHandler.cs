using CircloApp.Application.Features.Authentication.DTOs;
using CircloApp.Application.Interfaces;
using MediatR;

namespace CircloApp.Application.Features.Authentication.Queries.GetUserDetails
{
    public class GetUserInfoQueryHandler : IRequestHandler<GetUserInfoQuary, List<GetUserResponse>>
    {
        private readonly IUserRepository _userService;

        public GetUserInfoQueryHandler(IUserRepository userRepository)
        {
            _userService = userRepository;
        }

        public async Task<List<GetUserResponse>> Handle(GetUserInfoQuary request, CancellationToken cancellationToken)
        {
            var user = await _userService.SearchUserByUsername(request.charactors, cancellationToken);
            return user ?? [];
        }
    }
}
