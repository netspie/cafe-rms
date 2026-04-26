using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.ModifierGroups.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.ModifierGroups.Controllers;

[ApiController]
public sealed class DeleteModifierGroupController : ControllerBase
{
    [HttpDelete("/api/modifier-groups/{id:guid}")]
    [Authorize(Policy = Permissions.ModifiersManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await DeleteModifierGroup.Execute(id, db);
        return NoContent();
    }
}
