using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Events.UseCases;

[ApiController]
public sealed class CancelEventController : ControllerBase
{
    [HttpPost("/api/events/{id:guid}/cancel")]
    [Authorize(Policy = Permissions.EventsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] CancelEventRequest request,
        [FromServices] AppDbContext db)
    {
        await CancelEvent.Execute(id, request.Reason, db, DateTimeOffset.UtcNow);
        return NoContent();
    }
}

public sealed record CancelEventRequest(string? Reason);

public sealed class CancelEventValidator : AbstractValidator<CancelEventRequest>
{
    public CancelEventValidator()
    {
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}


public static class CancelEvent
{
    public static async Task Execute(Guid id, string? reason, AppDbContext db, DateTimeOffset now)
    {
        var ev = await db.Events.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Event not found.");

        if (ev.IsCancelled)
            throw new ConflictException("Event is already cancelled.");
        if (ev.IsClosed)
            throw new ConflictException("Cannot cancel a closed event.");

        ev.Cancel(now, reason);
        await db.SaveChangesAsync();
    }
}
