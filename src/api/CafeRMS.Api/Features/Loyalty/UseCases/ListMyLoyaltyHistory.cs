using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using CafeRMS.Api.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Loyalty.UseCases;

[ApiController]
public sealed class ListMyLoyaltyHistoryController : ControllerBase
{
    [HttpGet("/api/my/loyalty/history")]
    [Authorize(Policy = Policies.RequireGuest)]
    public Task<PagedResult<ListMyLoyaltyHistory.Item>> Handle(
        [FromQuery] ListMyLoyaltyHistoryRequest request,
        [FromServices] AppDbContext db) =>
        ListMyLoyaltyHistory.Execute(User.UserId,
            new ListMyLoyaltyHistory.Query { Page = request.Page, PageSize = request.PageSize, Sort = request.Sort },
            db);
}

public sealed record ListMyLoyaltyHistoryRequest(int Page = 1, int PageSize = 20, string? Sort = null);


public static class ListMyLoyaltyHistory
{
    public sealed record Query : PagedQuery;

    public sealed record Item(Guid Id, int Points, string? Reason, DateTimeOffset CreatedAt);

    public static async Task<PagedResult<Item>> Execute(Guid userId, Query query, AppDbContext db)
    {
        var sortable = new SortMap<LoyaltyPointLog>()
            .Add("createdAt", x => x.CreatedAt);

        return await db.LoyaltyPointLogs.IgnoreQueryFilters()
            .Where(x => x.UserId == userId)
            .ApplySort(query.Sort, sortable, defaultSortExpression: "-createdAt")
            .Select(x => new Item(x.Id, x.Points, x.Reason, x.CreatedAt))
            .ToPagedResultAsync(query);
    }
}
