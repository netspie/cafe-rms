using CafeRMS.Api.Features.Allergens.UseCases;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Allergens.Controllers;

[ApiController]
public sealed class ListAllergensController : ControllerBase
{
    [HttpGet("/api/allergens")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public Task<PagedResult<ListAllergens.Item>> Handle(
        [FromQuery] ListAllergensRequest request,
        [FromServices] AppDbContext db) =>
        ListAllergens.Execute(
            new ListAllergens.Query
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Sort = request.Sort,
                Name = request.Name
            },
            db);
}

public sealed record ListAllergensRequest(int Page = 1, int PageSize = 20, string? Sort = null, string? Name = null);
