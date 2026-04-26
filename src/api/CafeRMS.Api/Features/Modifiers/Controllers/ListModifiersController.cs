using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Modifiers.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Modifiers.Controllers;

[ApiController]
public sealed class ListModifiersController : ControllerBase
{
    [HttpGet("/api/modifiers")]
    [Authorize(Policy = Permissions.ModifiersManage)]
    public Task<PagedResult<ListModifiers.Item>> Handle(
        [FromQuery] ListModifiersRequest request,
        [FromServices] AppDbContext db) =>
        ListModifiers.Execute(
            new ListModifiers.Query
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Sort = request.Sort,
                Name = request.Name,
                ModifierGroupId = request.ModifierGroupId
            },
            db);
}

public sealed record ListModifiersRequest(
    int Page = 1,
    int PageSize = 20,
    string? Sort = null,
    string? Name = null,
    Guid? ModifierGroupId = null);
