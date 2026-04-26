using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.ModifierGroups.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.ModifierGroups.Controllers;

[ApiController]
public sealed class ListModifierGroupsController : ControllerBase
{
    [HttpGet("/api/modifier-groups")]
    [Authorize(Policy = Permissions.ModifiersManage)]
    public Task<PagedResult<ListModifierGroups.Item>> Handle(
        [FromQuery] ListModifierGroupsRequest request,
        [FromServices] AppDbContext db) =>
        ListModifierGroups.Execute(
            new ListModifierGroups.Query
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Sort = request.Sort,
                Name = request.Name
            },
            db);
}

public sealed record ListModifierGroupsRequest(int Page = 1, int PageSize = 20, string? Sort = null, string? Name = null);
