using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.PrintoutTemplates.UseCases;

[ApiController]
public sealed class ListPrintoutTemplatesController : ControllerBase
{
    [HttpGet("/api/printout-templates")]
    [Authorize(Policy = Permissions.PrintoutTemplatesManage)]
    public Task<PagedResult<ListPrintoutTemplates.Item>> Handle(
        [FromQuery] ListPrintoutTemplatesRequest request,
        [FromServices] AppDbContext db) =>
        ListPrintoutTemplates.Execute(
            new ListPrintoutTemplates.Query
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Sort = request.Sort,
                Name = request.Name
            },
            db);
}

public sealed record ListPrintoutTemplatesRequest(int Page = 1, int PageSize = 20, string? Sort = null, string? Name = null);


public static class ListPrintoutTemplates
{
    public sealed record Query : PagedQuery
    {
        public string? Name { get; init; }
    }

    public sealed record Item(Guid Id, string Name, string FileName, DateTimeOffset CreatedAt);

    public static async Task<PagedResult<Item>> Execute(Query query, AppDbContext db)
    {
        var sortable = new SortMap<PrintoutTemplate>()
            .Add("name", x => x.Name)
            .Add("createdAt", x => x.CreatedAt);

        var queryable = db.PrintoutTemplates.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var needle = query.Name.ToLower();
            queryable = queryable.Where(x => x.Name.ToLower().Contains(needle));
        }

        return await queryable
            .ApplySort(query.Sort, sortable, defaultSortExpression: "name")
            .Select(x => new Item(x.Id, x.Name, x.FileName, x.CreatedAt))
            .ToPagedResultAsync(query);
    }
}
