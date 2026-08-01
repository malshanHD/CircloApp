using CircloApp.Application.Features.Authentication.Commands;
using CircloApp.Application.Features.Authentication.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
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
        public async Task<ActionResult<RegisterUserResponse>> RegisterUser(RegisterUserRequest request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new RegisterUserCommand(request), cancellationToken);
            return Ok(response);
        }
    }
}
