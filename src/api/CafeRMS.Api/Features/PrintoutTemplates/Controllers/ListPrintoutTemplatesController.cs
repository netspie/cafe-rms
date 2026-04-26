using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.PrintoutTemplates.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.PrintoutTemplates.Controllers;

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
