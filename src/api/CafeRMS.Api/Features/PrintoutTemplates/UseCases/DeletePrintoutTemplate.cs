using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.PrintoutTemplates.UseCases;

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


public static class DeletePrintoutTemplate
{
    public static async Task Execute(Guid id, AppDbContext db)
    {
        var template = await db.PrintoutTemplates.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Printout template not found.");

        db.PrintoutTemplates.Remove(template);
        await db.SaveChangesAsync();
    }
}
