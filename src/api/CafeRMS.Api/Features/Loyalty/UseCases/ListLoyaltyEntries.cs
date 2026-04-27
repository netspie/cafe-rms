using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Loyalty.UseCases;

[ApiController]
public sealed class ListLoyaltyEntriesController : ControllerBase
{
    [HttpGet("/api/loyalty/entries")]
    [Authorize(Policy = Permissions.LoyaltyManage)]
    public Task<PagedResult<ListLoyaltyEntries.Item>> Handle(
        [FromQuery] ListLoyaltyEntriesRequest request,
        [FromServices] AppDbContext db) =>
        ListLoyaltyEntries.Execute(
            new ListLoyaltyEntries.Query
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Sort = request.Sort,
                UserId = request.UserId
            },
            db);
}

public sealed record ListLoyaltyEntriesRequest(int Page = 1, int PageSize = 20, string? Sort = null, Guid? UserId = null);


public static class ListLoyaltyEntries
{
    public sealed record Query : PagedQuery
    {
        public Guid? UserId { get; init; }
    }

    public sealed record Item(Guid Id, Guid UserId, int Points, string? Reason, DateTimeOffset CreatedAt);

    public static async Task<PagedResult<Item>> Execute(Query query, AppDbContext db)
    {
        var sortable = new SortMap<LoyaltyPointLog>()
            .Add("createdAt", x => x.CreatedAt);

        // Staff side: ICompanyOwned global filter applies — caller's company only.
        var queryable = db.LoyaltyPointLogs.AsQueryable();
        if (query.UserId is Guid userId)
            queryable = queryable.Where(x => x.UserId == userId);

        return await queryable
            .ApplySort(query.Sort, sortable, defaultSortExpression: "-createdAt")
            .Select(x => new Item(x.Id, x.UserId, x.Points, x.Reason, x.CreatedAt))
            .ToPagedResultAsync(query);
    }
}
