using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Events.UseCases;

[ApiController]
public sealed class ListEventsController : ControllerBase
{
    [HttpGet("/api/events")]
    [Authorize(Policy = Permissions.EventsManage)]
    public Task<PagedResult<ListEvents.Item>> Handle(
        [FromQuery] ListEventsRequest request,
        [FromServices] AppDbContext db) =>
        ListEvents.Execute(
            new ListEvents.Query
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Sort = request.Sort,
                Name = request.Name
            },
            db);
}

public sealed record ListEventsRequest(int Page = 1, int PageSize = 20, string? Sort = null, string? Name = null);


public static class ListEvents
{
    public sealed record Query : PagedQuery
    {
        public string? Name { get; init; }
    }

    public sealed record Item(Guid Id, string Name, EventStatus Status, DateTimeOffset CreatedAt);

    public static async Task<PagedResult<Item>> Execute(Query query, AppDbContext db)
    {
        var sortable = new SortMap<Event>()
            .Add("name", x => x.Name)
            .Add("createdAt", x => x.CreatedAt);

        var queryable = db.Events.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var needle = query.Name.ToLower();
            queryable = queryable.Where(x => x.Name.ToLower().Contains(needle));
        }

        return await queryable
            .ApplySort(query.Sort, sortable, defaultSortExpression: "-createdAt")
            .Select(x => new Item(x.Id, x.Name, x.Status, x.CreatedAt))
            .ToPagedResultAsync(query);
    }
}
