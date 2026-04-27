using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Events.UseCases;

[ApiController]
public sealed class AddEventDayController : ControllerBase
{
    [HttpPost("/api/events/{id:guid}/days")]
    [Authorize(Policy = Permissions.EventsManage)]
    public async Task<AddEventDayResponse> Handle(
        [FromRoute] Guid id,
        [FromBody] AddEventDayRequest request,
        [FromServices] AppDbContext db)
    {
        var result = await AddEventDay.Execute(id, request.Date, db);
        return new AddEventDayResponse(result.Id);
    }
}

public sealed record AddEventDayRequest(DateOnly Date);
public sealed record AddEventDayResponse(Guid Id);

public sealed class AddEventDayValidator : AbstractValidator<AddEventDayRequest>
{
    public AddEventDayValidator()
    {
        RuleFor(x => x.Date).NotEqual(default(DateOnly));
    }
}


public static class AddEventDay
{
    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Guid eventId, DateOnly date, AppDbContext db)
    {
        var ev = await db.Events.FirstOrDefaultAsync(x => x.Id == eventId)
            ?? throw new NotFoundException("Event not found.");

        if (ev.IsClosed || ev.IsCancelled)
            throw new ConflictException("Cannot add days to a closed or cancelled event.");

        var dupe = await db.EventDays.AnyAsync(x => x.EventId == eventId && x.Date == date);
        if (dupe)
            throw new ConflictException("That date is already on the event.");

        var day = EventDay.Create(eventId, date);
        db.EventDays.Add(day);
        await db.SaveChangesAsync();
        return new Result(day.Id);
    }
}
