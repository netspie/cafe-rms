using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.PriceGroups.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.PriceGroups.Controllers;

[ApiController]
public sealed class UpdatePriceGroupController : ControllerBase
{
    [HttpPut("/api/price-groups/{id:guid}")]
    [Authorize(Policy = Permissions.PricingManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdatePriceGroupRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new UpdatePriceGroup.Command(id, request.Name);
        await UpdatePriceGroup.Execute(command, db);
        return NoContent();
    }
}

public sealed record UpdatePriceGroupRequest(string Name);

public sealed class UpdatePriceGroupValidator : AbstractValidator<UpdatePriceGroupRequest>
{
    public UpdatePriceGroupValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
