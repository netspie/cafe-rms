using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.PriceGroups.UseCases;

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


public static class UpdatePriceGroup
{
    public sealed record Command(Guid Id, string Name);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var priceGroup = await db.PriceGroups.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Price group not found.");

        var nameTaken = await db.PriceGroups.AnyAsync(x => x.Name == command.Name && x.Id != command.Id);
        if (nameTaken)
            throw new ConflictException($"A price group named '{command.Name}' already exists.");

        priceGroup.Update(command.Name);
        await db.SaveChangesAsync();
    }
}
