using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Modifiers.UseCases;

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
        var command = new UpdateModifier.Command(id, request.Name);
        await UpdateModifier.Execute(command, db);
        return NoContent();
    }
}

public sealed record UpdateModifierRequest(string Name);

public sealed class UpdateModifierValidator : AbstractValidator<UpdateModifierRequest>
{
    public UpdateModifierValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}


public static class UpdateModifier
{
    public sealed record Command(Guid Id, string Name);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var modifier = await db.Modifiers.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Modifier not found.");

        var nameTaken = await db.Modifiers.AnyAsync(x =>
            x.ModifierGroupId == modifier.ModifierGroupId &&
            x.Name == command.Name &&
            x.Id != command.Id);
        if (nameTaken)
            throw new ConflictException($"A modifier named '{command.Name}' already exists in this group.");

        modifier.Update(command.Name);
        await db.SaveChangesAsync();
    }
}
