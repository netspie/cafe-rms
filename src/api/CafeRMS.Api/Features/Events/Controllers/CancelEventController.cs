using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Events.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Events.Controllers;

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
