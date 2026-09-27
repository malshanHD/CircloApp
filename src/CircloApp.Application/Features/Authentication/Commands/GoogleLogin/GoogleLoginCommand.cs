using CircloApp.Application.Features.Authentication.DTOs;
using MediatR;

namespace CircloApp.Application.Features.Authentication.Commands.GoogleLogin
{
    public record GoogleLoginCommand(GoogleLoginRequest Request) : IRequest<LoginResponse>;
}
