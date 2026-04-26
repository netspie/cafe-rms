using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.PromotionCodes.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.PromotionCodes.Controllers;

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
