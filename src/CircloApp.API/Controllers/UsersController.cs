using CircloApp.Application.Features.Authentication.Queries.GetUserDetails;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CircloApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{search}")]
        public async Task<IActionResult> GetEventExpenses([FromQuery] string q)
        {
            var result = await _mediator.Send(new GetUserInfoQuary(q));

            return Ok(result);
        }
    }
}
