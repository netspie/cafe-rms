using CafeRMS.Api.Features.Auth.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Auth.Controllers;

[ApiController]
public sealed class GetUserByIdController : ControllerBase
{
    [HttpGet("/api/users/{id:guid}")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<GetUserById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetUserById.Execute(db.CurrentCompanyId, id, db);
}
