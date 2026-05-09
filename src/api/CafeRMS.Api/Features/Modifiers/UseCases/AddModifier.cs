using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Modifiers.UseCases;

[ApiController]
public sealed class AddModifierController : ControllerBase
{
    [HttpPost("/api/modifiers")]
    [Authorize(Policy = Permissions.ModifiersManage)]
    public async Task<AddModifierResponse> Handle(
        [FromBody] AddModifierRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new AddModifier.Command(request.ModifierGroupId, request.Name);
        var result = await AddModifier.Execute(command, db);
        return new AddModifierResponse(result.Id);
    }
}

public sealed record AddModifierRequest(Guid ModifierGroupId, string Name);

public sealed record AddModifierResponse(Guid Id);

public sealed class AddModifierValidator : AbstractValidator<AddModifierRequest>
{
    public AddModifierValidator()
    {
        RuleFor(x => x.ModifierGroupId).NotEqual(Guid.Empty);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}


public static class AddModifier
{
    public sealed record Command(Guid ModifierGroupId, string Name);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        var groupExists = await db.ModifierGroups.AnyAsync(x => x.Id == command.ModifierGroupId);
        if (!groupExists)
            throw new NotFoundException("Modifier group not found.");

        var nameTaken = await db.Modifiers.AnyAsync(x =>
            x.ModifierGroupId == command.ModifierGroupId && x.Name == command.Name);
        if (nameTaken)
            throw new ConflictException($"A modifier named '{command.Name}' already exists in this group.");

        var modifier = Modifier.Create(command.Name, command.ModifierGroupId);
        db.Modifiers.Add(modifier);
        await db.SaveChangesAsync();
        return new Result(modifier.Id);
    }
}
