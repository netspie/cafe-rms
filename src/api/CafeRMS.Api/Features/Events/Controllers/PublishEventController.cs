using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Events.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Events.Controllers;

[ApiController]
public sealed class PublishEventController : ControllerBase
{
    [HttpPost("/api/events/{id:guid}/publish")]
    [Authorize(Policy = Permissions.EventsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await PublishEvent.Execute(id, db, DateTimeOffset.UtcNow);
        return NoContent();
    }
}
