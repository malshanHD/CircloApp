using CircloApp.Application.Features.PersonalExpenses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace CircloApp.API.Controllers;
[ApiController, Authorize, Route("api/personal-expense-budgets/{year:int}/{month:int}")]
public class PersonalExpenseBudgetsController(IMediator mediator) : ControllerBase
{
    private Guid CurrentUser => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;
    [HttpGet]
    public async Task<IActionResult> Get(int year, int month, CancellationToken ct) => CurrentUser == Guid.Empty ? Unauthorized() : Ok(await mediator.Send(new GetPersonalBudget(CurrentUser, year, month), ct));
    [HttpPut]
    public async Task<IActionResult> Put(int year, int month, BudgetRequest request, CancellationToken ct) => CurrentUser == Guid.Empty ? Unauthorized() : Ok(await mediator.Send(new SavePersonalBudget(CurrentUser, year, month, request), ct));
    [HttpDelete]
    public async Task<IActionResult> Delete(int year, int month, CancellationToken ct)
    {
        if (CurrentUser == Guid.Empty) return Unauthorized();
        await mediator.Send(new RemovePersonalBudget(CurrentUser, year, month), ct);
        return NoContent();
    }
}
