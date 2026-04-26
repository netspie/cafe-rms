using CafeRMS.Api.Features.Auth.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Auth.Controllers;

[ApiController]
public sealed class DeleteUserController : ControllerBase
{
    [HttpDelete("/api/users/{id:guid}")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        var command = new DeleteUser.Command(db.CurrentCompanyId, User.UserId, id);
        await DeleteUser.Execute(command, db);
        return NoContent();
    }
}
