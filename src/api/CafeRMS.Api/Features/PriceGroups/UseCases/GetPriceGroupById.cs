using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.PriceGroups.UseCases;

[ApiController]
public sealed class GetPriceGroupByIdController : ControllerBase
{
    [HttpGet("/api/price-groups/{id:guid}")]
    [Authorize(Policy = Permissions.PricingManage)]
    public async Task<GetPriceGroupById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetPriceGroupById.Execute(id, db);
}


public static class GetPriceGroupById
{
    public sealed record Result(Guid Id, string Name, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

    public static async Task<Result> Execute(Guid id, AppDbContext db)
    {
        var priceGroup = await db.PriceGroups
            .Where(x => x.Id == id)
            .Select(x => new Result(x.Id, x.Name, x.CreatedAt, x.UpdatedAt))
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Price group not found.");

        return priceGroup;
    }
}
