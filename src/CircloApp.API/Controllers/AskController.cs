using CircloApp.API.Models;
using CircloApp.Application.Features.AI_II.Queries.AskAi;
using CircloApp.Application.Interfaces.AI;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CircloApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AskController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IEmbeddingServiceII _embeddingServiceII;

        public AskController(IMediator mediator, IEmbeddingServiceII embeddingServiceII)
        {
            _mediator = mediator;
            _embeddingServiceII = embeddingServiceII;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] AskCircloRequest request, CancellationToken cancellationToken)
        {
            var query = new AskAiQuery(request.EventId, request.Question);

            var answer = await _mediator.Send(query, cancellationToken);

            return Ok(new { Answer = answer });
        }

        [HttpPost("embedding-test")]
        public async Task<IActionResult> TestEmbedding(
    [FromBody] AskCircloRequest request,
    CancellationToken cancellationToken)
        {
            var embedding = await _embeddingServiceII.GenerateEmbeddingAsync(
                request.Question,
                cancellationToken);

            return Ok(new
            {
                dimensions = embedding.Length,
                firstValues = embedding.Span[..5].ToArray()
            });
        }
    }
}
