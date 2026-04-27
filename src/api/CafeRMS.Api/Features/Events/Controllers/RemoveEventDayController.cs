using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Events.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Events.Controllers;

[ApiController]
public sealed class RemoveEventDayController : ControllerBase
{
    [HttpDelete("/api/events/{id:guid}/days/{dayId:guid}")]
    [Authorize(Policy = Permissions.EventsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromRoute] Guid dayId,
        [FromServices] AppDbContext db)
    {
        await RemoveEventDay.Execute(id, dayId, db);
        return NoContent();
    }
}
