using CafeRMS.Api.Features.Auth.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Auth.Controllers;

[ApiController]
public sealed class AssignRoleController : ControllerBase
{
    [HttpPost("/api/users/{userId:guid}/roles/{roleId:guid}")]
    [Authorize(Policy = Permissions.RolesManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid userId,
        [FromRoute] Guid roleId,
        [FromServices] AppDbContext db)
    {
        await AssignRole.Execute(new AssignRole.Command(db.CurrentCompanyId, userId, roleId), db);
        return NoContent();
    }
}
