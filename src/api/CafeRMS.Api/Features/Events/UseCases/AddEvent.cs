using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Events.UseCases;

[ApiController]
public sealed class AddEventController : ControllerBase
{
    [HttpPost("/api/events")]
    [Authorize(Policy = Permissions.EventsManage)]
    public async Task<AddEventResponse> Handle(
        [FromBody] AddEventRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new AddEvent.Command(db.CurrentCompanyId, request.Name, request.Description, request.ImageUrl, request.ProductListId, request.PriceGroupId);
        var result = await AddEvent.Execute(command, db);
        return new AddEventResponse(result.Id);
    }
}

public sealed record AddEventRequest(string Name, string? Description, string? ImageUrl, Guid? ProductListId, Guid? PriceGroupId);
public sealed record AddEventResponse(Guid Id);

public sealed class AddEventValidator : AbstractValidator<AddEventRequest>
{
    public AddEventValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.ImageUrl).MaximumLength(500);
    }
}


public static class AddEvent
{
    public sealed record Command(
        Guid CompanyId,
        string Name,
        string? Description,
        string? ImageUrl,
        Guid? ProductListId,
        Guid? PriceGroupId);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        if (command.CompanyId == Guid.Empty)
            throw new ForbiddenException("A company context is required to create an event.");

        if (command.ProductListId is { } plId)
        {
            var exists = await db.ProductLists.AnyAsync(x => x.Id == plId);
            if (!exists)
                throw new NotFoundException("Product list not found.");
        }
        if (command.PriceGroupId is { } pgId)
        {
            var exists = await db.PriceGroups.AnyAsync(x => x.Id == pgId);
            if (!exists)
                throw new NotFoundException("Price group not found.");
        }

        var ev = Event.Create(command.Name, command.CompanyId, command.Description, command.ImageUrl, command.ProductListId, command.PriceGroupId);
        db.Events.Add(ev);
        await db.SaveChangesAsync();
        return new Result(ev.Id);
    }
}
