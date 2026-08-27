using CircloApp.Application.Features.AI.Commands;
using CircloApp.Application.Features.AI.Queries.GetEventAiAnalysis;
using CircloApp.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CircloApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IAiService _service;

        public AIController(IMediator mediator, IAiService aiService)
        {
            _mediator = mediator;   
            _service = aiService;
        }

        [HttpGet("{eventId:guid}")]
        public async Task<IActionResult> GetEventExpenses(Guid eventId)
        {
            var result = await _mediator.Send(new GetEventExpensesSummaryCommand(eventId));

            return Ok(result);
        }

        [HttpGet("events/{eventId:guid}/categories")]
        public async Task<IActionResult> CategorizeEventExpenses(Guid eventId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetEventAiAnalysisQuery(eventId));

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Test(string prompt)
        {
            var res = await _service.GenerateAsync(prompt);

            return Ok(res);
        }
    }

    public class AiTestRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }
}
