using CircloApp.Application.Features.AI.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CircloApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AIController(IMediator mediator)
        {
            _mediator = mediator;   
        }

        [HttpGet("{eventId:guid}")]
        public async Task<IActionResult> GetEventExpenses(Guid eventId)
        {
            var result = await _mediator.Send(new GetEventExpensesSummaryCommand(eventId));

            return Ok(result);
        }
    }
}
