using CircloApp.Application.Features.PersonalExpenses.Commands.AddExpenses;
using CircloApp.Application.Features.PersonalExpenses.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CircloApp.Application.Features.PersonalExpenses;

namespace CircloApp.API.Controllers
{
    [Route("api/[controller]")]
    [Route("api/personal-expenses")]
    [ApiController]
    [Authorize]
    public class PersonalExpensesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PersonalExpensesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        private Guid CurrentUser => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard([FromQuery] int? year, [FromQuery] int? month, CancellationToken ct) => CurrentUser == Guid.Empty
            ? Unauthorized() : Ok(await _mediator.Send(new GetPersonalDashboard(CurrentUser, year, month), ct));
        [HttpGet("analysis")]
        public async Task<IActionResult> Analysis([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate, CancellationToken ct) => CurrentUser == Guid.Empty
            ? Unauthorized() : Ok(await _mediator.Send(new GetPersonalAnalysis(CurrentUser, fromDate, toDate), ct));
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] ExpenseFilter filter, CancellationToken ct) => CurrentUser == Guid.Empty
            ? Unauthorized() : Ok(await _mediator.Send(new ListPersonalExpenses(CurrentUser, filter), ct));
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct) => CurrentUser == Guid.Empty
            ? Unauthorized() : Ok(await _mediator.Send(new GetPersonalExpense(CurrentUser, id), ct));
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, CreatePersonalExpenseRequest request, CancellationToken ct) => CurrentUser == Guid.Empty
            ? Unauthorized() : Ok(await _mediator.Send(new UpdatePersonalExpense(CurrentUser, id, request), ct));
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            if (CurrentUser == Guid.Empty) return Unauthorized();
            await _mediator.Send(new DeletePersonalExpense(CurrentUser, id), ct);
            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(typeof(PersonalExpensesResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreatePersonalExpenseRequest request, CancellationToken cancellationToken)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(!Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();  
            }

            var response = await _mediator.Send(new AddExpenseCommand(request, userId), cancellationToken);

            return StatusCode(StatusCodes.Status201Created, response);
        }
    }
}
