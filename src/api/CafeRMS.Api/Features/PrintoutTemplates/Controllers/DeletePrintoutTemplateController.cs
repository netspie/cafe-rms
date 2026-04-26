using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.PrintoutTemplates.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.PrintoutTemplates.Controllers;

[ApiController]
public sealed class DeletePrintoutTemplateController : ControllerBase
{
    [HttpDelete("/api/printout-templates/{id:guid}")]
    [Authorize(Policy = Permissions.PrintoutTemplatesManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await DeletePrintoutTemplate.Execute(id, db);
        return NoContent();
    }
}
