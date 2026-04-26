using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.ModifierGroups.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.ModifierGroups.Controllers;

[ApiController]
public sealed class UpdateModifierGroupController : ControllerBase
{
    [HttpPut("/api/modifier-groups/{id:guid}")]
    [Authorize(Policy = Permissions.ModifiersManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdateModifierGroupRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new UpdateModifierGroup.Command(id, request.Name);
        await UpdateModifierGroup.Execute(command, db);
        return NoContent();
    }
}

public sealed record UpdateModifierGroupRequest(string Name);

public sealed class UpdateModifierGroupValidator : AbstractValidator<UpdateModifierGroupRequest>
{
    public UpdateModifierGroupValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
