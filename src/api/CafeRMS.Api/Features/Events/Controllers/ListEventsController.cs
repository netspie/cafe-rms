using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Events.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Events.Controllers;

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
