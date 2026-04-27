using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.ModifierGroups.UseCases;

[ApiController]
public sealed class AddModifierGroupController : ControllerBase
{
    [HttpPost("/api/modifier-groups")]
    [Authorize(Policy = Permissions.ModifiersManage)]
    public async Task<AddModifierGroupResponse> Handle(
        [FromBody] AddModifierGroupRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new AddModifierGroup.Command(db.CurrentCompanyId, request.Name);
        var result = await AddModifierGroup.Execute(command, db);
        return new AddModifierGroupResponse(result.Id);
    }
}

public sealed record AddModifierGroupRequest(string Name);

public sealed record AddModifierGroupResponse(Guid Id);

public sealed class AddModifierGroupValidator : AbstractValidator<AddModifierGroupRequest>
{
    public AddModifierGroupValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}


public static class AddModifierGroup
{
    public sealed record Command(Guid CompanyId, string Name);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        if (command.CompanyId == Guid.Empty)
            throw new ForbiddenException("A company context is required to create a modifier group.");

        var nameTaken = await db.ModifierGroups.AnyAsync(x => x.Name == command.Name);
        if (nameTaken)
            throw new ConflictException($"A modifier group named '{command.Name}' already exists.");

        var group = ModifierGroup.Create(command.Name, command.CompanyId);
        db.ModifierGroups.Add(group);
        await db.SaveChangesAsync();
        return new Result(group.Id);
    }
}
