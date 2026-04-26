using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Modifiers.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Modifiers.Controllers;

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
