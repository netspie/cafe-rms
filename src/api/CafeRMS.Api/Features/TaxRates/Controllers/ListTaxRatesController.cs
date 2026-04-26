using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.TaxRates.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.TaxRates.Controllers;

[ApiController]
public sealed class ListTaxRatesController : ControllerBase
{
    [HttpGet("/api/tax-rates")]
    [Authorize(Policy = Permissions.TaxRatesManage)]
    public Task<PagedResult<ListTaxRates.Item>> Handle(
        [FromQuery] ListTaxRatesRequest request,
        [FromServices] AppDbContext db) =>
        ListTaxRates.Execute(
            new ListTaxRates.Query
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Sort = request.Sort,
                Name = request.Name
            },
            db);
}

public sealed record ListTaxRatesRequest(int Page = 1, int PageSize = 20, string? Sort = null, string? Name = null);
