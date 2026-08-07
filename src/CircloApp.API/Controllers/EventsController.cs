using CircloApp.Application.Features.Events.Commands;
using CircloApp.Application.Features.Events.Commands.InviteEvent;
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

        [HttpGet("{eventId:guid}")]
        public async Task<IActionResult> GetEventDetails(Guid eventId)
        {
            var result = await _mediator.Send(new GetEventDetailsQuery(eventId));

            return Ok(result);
        }

        [HttpPost("{eventId:guid}/members")]
        public async Task<IActionResult> InviteMember(
        Guid eventId,
        InviteRequest request)
        {
            var memberId = await _mediator.Send(
                new EventInviteCommand(eventId, request));

            return Ok(memberId);
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User Id not found");

            return Guid.Parse(userId);
        }
    }
}
