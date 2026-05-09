using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.PriceGroups.UseCases;

[ApiController]
public sealed class AddPriceGroupController : ControllerBase
{
    [HttpPost("/api/price-groups")]
    [Authorize(Policy = Permissions.PricingManage)]
    public async Task<AddPriceGroupResponse> Handle(
        [FromBody] AddPriceGroupRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new AddPriceGroup.Command(request.Name);
        var result = await AddPriceGroup.Execute(command, db);
        return new AddPriceGroupResponse(result.Id);
    }
}

public sealed record AddPriceGroupRequest(string Name);

public sealed record AddPriceGroupResponse(Guid Id);

public sealed class AddPriceGroupValidator : AbstractValidator<AddPriceGroupRequest>
{
    public AddPriceGroupValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public static class AddPriceGroup
{
    public sealed record Command(string Name);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        var nameTaken = await db.PriceGroups.AnyAsync(x => x.Name == command.Name);
        if (nameTaken)
            throw new ConflictException($"A price group named '{command.Name}' already exists.");

        var priceGroup = PriceGroup.Create(command.Name);
        db.PriceGroups.Add(priceGroup);
        await db.SaveChangesAsync();
        return new Result(priceGroup.Id);
    }
}
