using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.PromotionCodes.UseCases;

[ApiController]
public sealed class ListPromotionCodesController : ControllerBase
{
    [HttpGet("/api/promotion-codes")]
    [Authorize(Policy = Permissions.PromotionsManage)]
    public Task<PagedResult<ListPromotionCodes.Item>> Handle(
        [FromQuery] ListPromotionCodesRequest request,
        [FromServices] AppDbContext db) =>
        ListPromotionCodes.Execute(
            new ListPromotionCodes.Query
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Sort = request.Sort,
                Code = request.Code
            },
            db);
}

public sealed record ListPromotionCodesRequest(int Page = 1, int PageSize = 20, string? Sort = null, string? Code = null);


public static class ListPromotionCodes
{
    public sealed record Query : PagedQuery
    {
        public string? Code { get; init; }
    }

    public sealed record Item(
        Guid Id,
        string Code,
        decimal DiscountPercentage,
        DateTimeOffset? ValidFrom,
        DateTimeOffset? ValidUntil,
        int? MaxUses,
        int UsesCount,
        DateTimeOffset CreatedAt);

    public static async Task<PagedResult<Item>> Execute(Query query, AppDbContext db)
    {
        var sortable = new SortMap<PromotionCode>()
            .Add("code", x => x.Code)
            .Add("createdAt", x => x.CreatedAt)
            .Add("validUntil", x => x.ValidUntil);

        var queryable = db.PromotionCodes.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Code))
        {
            var needle = query.Code.ToLower();
            queryable = queryable.Where(x => x.Code.ToLower().Contains(needle));
        }

        return await queryable
            .ApplySort(query.Sort, sortable, defaultSortExpression: "code")
            .Select(x => new Item(
                x.Id, x.Code, x.DiscountPercentage,
                x.ValidFrom, x.ValidUntil, x.MaxUses, x.UsesCount,
                x.CreatedAt))
            .ToPagedResultAsync(query);
    }
}
