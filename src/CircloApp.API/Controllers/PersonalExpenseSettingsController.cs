using CircloApp.Application.Features.PersonalExpenses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace CircloApp.API.Controllers;
[ApiController, Authorize, Route("api/personal-expense-settings")]
public class PersonalExpenseSettingsController(IMediator mediator) : ControllerBase
{
    private Guid CurrentUser => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => CurrentUser == Guid.Empty ? Unauthorized() : Ok(await mediator.Send(new GetPersonalSettings(CurrentUser), ct));
    [HttpPut]
    public async Task<IActionResult> Put(SettingsRequest request, CancellationToken ct) => CurrentUser == Guid.Empty ? Unauthorized() : Ok(await mediator.Send(new SavePersonalSettings(CurrentUser, request), ct));
}
