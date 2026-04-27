using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Loyalty.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Loyalty.Controllers;

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
