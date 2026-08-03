using CircloApp.Application.Features.Authentication.DTOs;
using MediatR;

namespace CircloApp.Application.Features.Authentication.Commands.VerifyOtp
{
    public record VerifyOtpCommand(VerifyOtpRequest Request) : IRequest<VerifyOtpResponse>;
}
