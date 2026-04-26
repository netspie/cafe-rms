using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.ModifierGroups.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.ModifierGroups.Controllers;

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
