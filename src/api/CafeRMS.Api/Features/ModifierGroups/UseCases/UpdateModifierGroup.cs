using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.ModifierGroups.UseCases;

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


public static class UpdateModifierGroup
{
    public sealed record Command(Guid Id, string Name);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var group = await db.ModifierGroups.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Modifier group not found.");

        var nameTaken = await db.ModifierGroups.AnyAsync(x => x.Name == command.Name && x.Id != command.Id);
        if (nameTaken)
            throw new ConflictException($"A modifier group named '{command.Name}' already exists.");

        group.Update(command.Name);
        await db.SaveChangesAsync();
    }
}
