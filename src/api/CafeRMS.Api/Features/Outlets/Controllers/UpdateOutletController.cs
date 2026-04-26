using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Outlets.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Outlets.Controllers;

[ApiController]
public sealed class UpdateOutletController : ControllerBase
{
    [HttpPut("/api/outlets/{id:guid}")]
    [Authorize(Policy = Permissions.OutletManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdateOutletRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new UpdateOutlet.Command(
            id,
            request.DisplayName,
            request.StreetAddress,
            request.Phone,
            request.TimeZone,
            request.Currency,
            request.LogoUrl);
        await UpdateOutlet.Execute(command, db);
        return NoContent();
    }
}

public sealed record UpdateOutletRequest(
    string DisplayName,
    string StreetAddress,
    string Phone,
    string TimeZone,
    Currency Currency,
    string? LogoUrl);

public sealed class UpdateOutletValidator : AbstractValidator<UpdateOutletRequest>
{
    public UpdateOutletValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StreetAddress).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(50);
        RuleFor(x => x.TimeZone).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LogoUrl).MaximumLength(500);
    }
}
