using CircloApp.Application.Features.Authentication.Commands;
using CircloApp.Application.Features.Authentication.Commands.Login;
using CircloApp.Application.Features.Authentication.Commands.VerifyOtp;
using CircloApp.Application.Features.Authentication.DTOs;
using CircloApp.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CircloApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IMediator mediator, ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> RegisterUser(RegisterUserRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"New registration Request receieved from {request.Email}");
            var response = await _mediator.Send(new RegisterUserCommand(request), cancellationToken);
            return Ok(ApiResponse<RegisterResponse>.SuccessResponse(response, "User registered successfully."));
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new LoginCommand(request), cancellationToken);
            return Ok(ApiResponse<LoginResponse>.SuccessResponse(response, "User logged in successfully."));
        }

        [HttpPost("verify-email")]
        public async Task<ActionResult<VerifyOtpResponse>> VerifyEmail(VerifyOtpRequest request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new VerifyOtpCommand(request), cancellationToken);
            return Ok(ApiResponse<VerifyOtpResponse>.SuccessResponse(response, "Email verified successfully."));
        }
    }
}
