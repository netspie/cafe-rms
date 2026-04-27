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
        var command = new AddModifier.Command(db.CurrentCompanyId, request.ModifierGroupId, request.Name, request.PriceDelta);
        var result = await AddModifier.Execute(command, db);
        return new AddModifierResponse(result.Id);
    }
}

public sealed record AddModifierRequest(Guid ModifierGroupId, string Name, decimal PriceDelta);

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
    public sealed record Command(Guid CompanyId, Guid ModifierGroupId, string Name, decimal PriceDelta);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        if (command.CompanyId == Guid.Empty)
            throw new ForbiddenException("A company context is required to create a modifier.");

        // Global filter on ModifierGroup auto-scopes by current company; missing or
        // cross-company group → 404.
        var groupExists = await db.ModifierGroups.AnyAsync(x => x.Id == command.ModifierGroupId);
        if (!groupExists)
            throw new NotFoundException("Modifier group not found.");

        var nameTaken = await db.Modifiers.AnyAsync(x =>
            x.ModifierGroupId == command.ModifierGroupId && x.Name == command.Name);
        if (nameTaken)
            throw new ConflictException($"A modifier named '{command.Name}' already exists in this group.");

        var modifier = Modifier.Create(command.Name, command.ModifierGroupId, command.CompanyId, command.PriceDelta);
        db.Modifiers.Add(modifier);
        await db.SaveChangesAsync();
        return new Result(modifier.Id);
    }
}
