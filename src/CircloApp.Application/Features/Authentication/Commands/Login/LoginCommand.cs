using CircloApp.Application.Features.Authentication.DTOs;
using MediatR;

namespace CircloApp.Application.Features.Authentication.Commands.Login
{
    public record LoginCommand(LoginRequest Request) : IRequest<LoginResponse>;
}
