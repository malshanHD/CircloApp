using CircloApp.Application.Features.Events.JoinRequests;
using CircloApp.Application.Features.Events.Commands;
using CircloApp.Application.Features.Events.DTOs;
using CircloApp.Application.Features.Events.Queries.GetEventDetails;
using CircloApp.Application.Features.Events.Queries.GetMyEvents;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CircloApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EventsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EventsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateEventRequest request)
        {
            var userId = GetCurrentUserId();

            var response = await _mediator.Send(new CreateEventCommand(userId, request));

            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyEvents([FromQuery] GetMyEventsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("join-requests")]
        public async Task<IActionResult> GetJoinRequests(CancellationToken ct) =>
            Ok(await _mediator.Send(new GetJoinRequestsQuery(), ct));

        [HttpGet("{eventId:guid}/join-request")]
        public async Task<IActionResult> GetJoinStatus(Guid eventId, CancellationToken ct) =>
            Ok(await _mediator.Send(new GetJoinStatusQuery(eventId), ct));

        [HttpPost("{eventId:guid}/join-requests")]
        public async Task<IActionResult> RequestJoin(Guid eventId, CancellationToken ct) =>
            Ok(await _mediator.Send(new RequestJoinCommand(eventId), ct));

        [HttpPost("{eventId:guid}/join-requests/{userId:guid}/approve")]
        public async Task<IActionResult> ApproveJoin(Guid eventId, Guid userId, CancellationToken ct) =>
            Ok(await _mediator.Send(new ApproveJoinCommand(eventId, userId), ct));
        [HttpGet("{eventId:guid}")]
        public async Task<IActionResult> GetEventDetails(Guid eventId)
        {
            var result = await _mediator.Send(new GetEventDetailsQuery(eventId));

            return Ok(result);
        }

        // Old clients must not bypass the administrator approval step.
        [HttpPost("{eventId:guid}/members")]
        [HttpPost("{eventId:guid}/accept-invitation")]
        public IActionResult RetiredInvitation(Guid eventId) =>
            StatusCode(410, new { message = "Use the shared event link to request access. An event admin must approve membership." });
        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User Id not found");

            return Guid.Parse(userId);
        }
    }
}
