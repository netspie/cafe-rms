using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Loyalty.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Loyalty.Controllers;

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
