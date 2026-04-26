using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.SalesChannels.UseCases;

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
