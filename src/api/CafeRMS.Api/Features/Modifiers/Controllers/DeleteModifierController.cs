using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Modifiers.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Modifiers.Controllers;

[ApiController]
public sealed class DeleteModifierController : ControllerBase
{
    [HttpDelete("/api/modifiers/{id:guid}")]
    [Authorize(Policy = Permissions.ModifiersManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await DeleteModifier.Execute(id, db);
        return NoContent();
    }
}
