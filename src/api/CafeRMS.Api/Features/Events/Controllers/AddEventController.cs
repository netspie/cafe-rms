using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Events.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Events.Controllers;

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
