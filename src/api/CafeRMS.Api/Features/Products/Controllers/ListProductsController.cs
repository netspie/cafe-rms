using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Products.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Products.Controllers;

[ApiController]
public sealed class ListProductsController : ControllerBase
{
    [HttpGet("/api/products")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public Task<PagedResult<ListProducts.Item>> Handle(
        [FromQuery] ListProductsRequest request,
        [FromServices] AppDbContext db) =>
        ListProducts.Execute(
            new ListProducts.Query
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Sort = request.Sort,
                Name = request.Name,
                Barcode = request.Barcode,
                TaxRateId = request.TaxRateId
            },
            db);
}

public sealed record ListProductsRequest(
    int Page = 1,
    int PageSize = 20,
    string? Sort = null,
    string? Name = null,
    string? Barcode = null,
    Guid? TaxRateId = null);
