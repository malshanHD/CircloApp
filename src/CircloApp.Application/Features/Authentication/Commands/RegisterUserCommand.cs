using CircloApp.Application.Features.Authentication.DTOs;
using MediatR;

namespace CircloApp.Application.Features.Authentication.Commands
{
    public record RegisterUserCommand(RegisterUserRequest Request) : IRequest<RegisterUserResponse>;
}
