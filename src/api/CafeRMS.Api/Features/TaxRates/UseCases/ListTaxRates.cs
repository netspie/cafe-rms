using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.TaxRates.UseCases;

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


public static class ListTaxRates
{
    public sealed record Query : PagedQuery
    {
        public string? Name { get; init; }
    }

    public sealed record Item(Guid Id, string Name, string Description, decimal Rate, DateTimeOffset CreatedAt);

    public static async Task<PagedResult<Item>> Execute(Query query, AppDbContext db)
    {
        var sortable = new SortMap<TaxRate>()
            .Add("name", x => x.Name)
            .Add("rate", x => x.Rate)
            .Add("createdAt", x => x.CreatedAt);

        var queryable = db.TaxRates.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var needle = query.Name.ToLower();
            queryable = queryable.Where(x => x.Name.ToLower().Contains(needle));
        }

        return await queryable
            .ApplySort(query.Sort, sortable, defaultSortExpression: "name")
            .Select(x => new Item(x.Id, x.Name, x.Description, x.Rate, x.CreatedAt))
            .ToPagedResultAsync(query);
    }
}
