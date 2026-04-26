using CafeRMS.Api.Features.Auth.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Auth.Controllers;

[ApiController]
public sealed class DeleteRoleController : ControllerBase
{
    [HttpDelete("/api/roles/{id:guid}")]
    [Authorize(Policy = Permissions.RolesManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await DeleteRole.Execute(id, db);
        return NoContent();
    }
}
