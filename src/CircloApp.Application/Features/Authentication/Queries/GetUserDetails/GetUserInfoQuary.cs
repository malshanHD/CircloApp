using CircloApp.Application.Features.Authentication.DTOs;
using MediatR;

namespace CircloApp.Application.Features.Authentication.Queries.GetUserDetails
{
    public record class GetUserInfoQuary(string charactors) : IRequest<List<GetUserResponse>>;
}
