using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.SalesChannels.UseCases;

[ApiController]
public sealed class GetSalesChannelByIdController : ControllerBase
{
    [HttpGet("/api/sales-channels/{id:guid}")]
    [Authorize]
    public async Task<GetSalesChannelById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetSalesChannelById.Execute(id, db);
}


public static class GetSalesChannelById
{
    public sealed record Result(
        Guid Id,
        string Name,
        bool IsTakeout,
        IReadOnlyList<Guid> PriceGroupIds,
        DateTimeOffset CreatedAt,
        DateTimeOffset? UpdatedAt);

    public static async Task<Result> Execute(Guid id, AppDbContext db)
    {
        var channel = await db.SalesChannels.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Sales channel not found.");

        var priceGroupIds = await db.SalesChannelPriceGroups
            .Where(x => x.SalesChannelId == id)
            .Select(x => x.PriceGroupId)
            .ToListAsync();

        return new Result(channel.Id, channel.Name, channel.IsTakeout, priceGroupIds, channel.CreatedAt, channel.UpdatedAt);
    }
}
