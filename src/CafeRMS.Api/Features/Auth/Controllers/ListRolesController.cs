using CafeRMS.Api.Features.Auth.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Auth.Controllers;

[ApiController]
public sealed class ListRolesController : ControllerBase
{
    [HttpGet("/api/roles")]
    [Authorize(Policy = Permissions.RolesManage)]
    public async Task<IReadOnlyList<ListRoles.Item>> Handle([FromServices] AppDbContext db) =>
        await ListRoles.Execute(db);
}
