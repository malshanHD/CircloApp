using CircloApp.Application.Features.Authentication.Commands;
using CircloApp.Application.Features.Authentication.Commands.Login;
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

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> RegisterUser(RegisterUserRequest request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new RegisterUserCommand(request), cancellationToken);
            return Ok(ApiResponse<RegisterResponse>.SuccessResponse(response, "User registered successfully."));
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new LoginCommand(request), cancellationToken);
            return Ok(ApiResponse<LoginResponse>.SuccessResponse(response, "User logged in successfully."));
        }
    }
}
