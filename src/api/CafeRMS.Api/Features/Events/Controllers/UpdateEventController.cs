using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Events.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Events.Controllers;

[ApiController]
public sealed class UpdateEventController : ControllerBase
{
    [HttpPut("/api/events/{id:guid}")]
    [Authorize(Policy = Permissions.EventsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdateEventRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new UpdateEvent.Command(id, request.Name, request.Description, request.ImageUrl, request.ProductListId, request.PriceGroupId);
        await UpdateEvent.Execute(command, db);
        return NoContent();
    }
}

public sealed record UpdateEventRequest(string Name, string? Description, string? ImageUrl, Guid? ProductListId, Guid? PriceGroupId);

public sealed class UpdateEventValidator : AbstractValidator<UpdateEventRequest>
{
    public UpdateEventValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.ImageUrl).MaximumLength(500);
    }
}
