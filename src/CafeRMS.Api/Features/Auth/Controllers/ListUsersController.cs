using CafeRMS.Api.Features.Auth.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Auth.Controllers;

[ApiController]
public sealed class ListUsersController : ControllerBase
{
    [HttpGet("/api/users")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IReadOnlyList<ListUsers.Item>> Handle([FromServices] AppDbContext db) =>
        await ListUsers.Execute(db.CurrentCompanyId, db);
}
