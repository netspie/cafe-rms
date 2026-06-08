using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.PrintoutTemplates.UseCases;

[ApiController]
public sealed class DownloadPrintoutTemplateController : ControllerBase
{
    [HttpGet("/api/printout-templates/{id:guid}/file")]
    [Authorize(Policy = Permissions.PrintoutTemplatesManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        var template = await db.PrintoutTemplates
            .Where(x => x.Id == id)
            .Select(x => new { x.FileName, x.ContentType, x.FileContent })
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Printout template not found.");

        var contentType = string.IsNullOrWhiteSpace(template.ContentType)
            ? PrintoutFile.DocxContentType
            : template.ContentType;
        return File(template.FileContent, contentType, template.FileName);
    }
}
