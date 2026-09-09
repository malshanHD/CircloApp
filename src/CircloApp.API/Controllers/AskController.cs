using CircloApp.API.Models;
using CircloApp.Application.Features.AI_II.Queries.AskAi;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CircloApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AskController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AskController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] AskCircloRequest request, CancellationToken cancellationToken)
        {
            var query = new AskAiQuery(request.Question);

            var answer = await _mediator.Send(query, cancellationToken);

            return Ok(new { Answer = answer });
        }
    }
}
