using CircloApp.Application.Features.PersonalExpenses.DTOs;
using CircloApp.Application.Features.PersonalExpenses.Query.GetExpensesCategories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CircloApp.API.Controllers
{
    [Route("api/[controller]")]
    [Route("api/personal-expense-categories")]
    [ApiController]
    [Authorize]
    public class PersonalExpenseCategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PersonalExpenseCategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(
            typeof(IReadOnlyList<PersonalExpenseCategoryResponse>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll(
            CancellationToken cancellationToken)
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var categories = await _mediator.Send(
                new GetPersonalExpenseCategoriesQuery(userId),
                cancellationToken);

            return Ok(categories);
        }
    }
}
