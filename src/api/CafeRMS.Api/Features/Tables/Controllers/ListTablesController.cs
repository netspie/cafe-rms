using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Tables.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Tables.Controllers;

[ApiController]
public sealed class ListTablesController : ControllerBase
{
    [HttpGet("/api/tables")]
    [Authorize(Policy = Permissions.TablesManage)]
    public Task<PagedResult<ListTables.Item>> Handle(
        [FromQuery] ListTablesRequest request,
        [FromServices] AppDbContext db) =>
        ListTables.Execute(
            new ListTables.Query
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Sort = request.Sort,
                Name = request.Name
            },
            db);
}

public sealed record ListTablesRequest(int Page = 1, int PageSize = 20, string? Sort = null, string? Name = null);
