using CircloApp.Application.Features.Events.Queries.GetEventDetails;
using CircloApp.Application.Features.Expenses.Commands.AddExpenses;
using CircloApp.Application.Features.Expenses.DTOs;
using CircloApp.Application.Features.Expenses.Queries.GetEventExpenses;
using CircloApp.Application.Features.Expenses.Queries.GetUserExpenses;
using CircloApp.Application.Features.Expenses.Queries.GetUserMonthlyExpenses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CircloApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExpensesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ExpensesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("{eventId:guid}/expenses")]
        public async Task<ActionResult> AddExpenses(Guid eventId, CreateExpensesRequest createExpensesRequest)
        {
            var expenses = await _mediator.Send(new AddExpensesCommand(createExpensesRequest, eventId));
            return Ok(expenses);
        }

        [HttpGet("{eventId:guid}")]
        public async Task<IActionResult> GetEventExpenses(Guid eventId)
        {
            var result = await _mediator.Send(new GetEventExpensesQuery(eventId));

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserAllExpenses()
        {
            var result = await _mediator.Send(new GetUserExpenses());

            return Ok(result);
        }

        [HttpGet("{year:int}")]
        public async Task<IActionResult> GetUserExpensesByMonthly(int year)
        {
            var result = await _mediator.Send(new GetUserMonthlyExpensesQuery(year));
            return Ok(result);
        }
    }
}
