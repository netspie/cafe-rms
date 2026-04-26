using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Modifiers.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Modifiers.Controllers;

[ApiController]
public sealed class UpdateModifierController : ControllerBase
{
    [HttpPut("/api/modifiers/{id:guid}")]
    [Authorize(Policy = Permissions.ModifiersManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdateModifierRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new UpdateModifier.Command(id, request.Name, request.PriceDelta);
        await UpdateModifier.Execute(command, db);
        return NoContent();
    }
}

public sealed record UpdateModifierRequest(string Name, decimal PriceDelta);

public sealed class UpdateModifierValidator : AbstractValidator<UpdateModifierRequest>
{
    public UpdateModifierValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
