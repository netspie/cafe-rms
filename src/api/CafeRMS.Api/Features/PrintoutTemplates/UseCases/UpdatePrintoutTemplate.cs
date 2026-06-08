using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.PrintoutTemplates.UseCases;

[ApiController]
public sealed class UpdatePrintoutTemplateController : ControllerBase
{
    [HttpPut("/api/printout-templates/{id:guid}")]
    [Authorize(Policy = Permissions.PrintoutTemplatesManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromForm] string name,
        IFormFile? file,
        [FromServices] AppDbContext db)
    {
        // file is optional on update — null means keep the existing document, change the name only
        var hasNewFile = file is { Length: > 0 };
        var fileName = hasNewFile ? file!.FileName : null;
        var contentType = hasNewFile ? file!.ContentType : null;
        var fileContent = hasNewFile ? await PrintoutFile.ReadAsync(file) : null;

        var command = new UpdatePrintoutTemplate.Command(id, name, fileName, contentType, fileContent);
        await UpdatePrintoutTemplate.Execute(command, db);
        return NoContent();
    }
}


public static class UpdatePrintoutTemplate
{
    public sealed record Command(Guid Id, string Name, string? FileName, string? ContentType, byte[]? FileContent);

    public static async Task Execute(Command command, AppDbContext db)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new DomainException("Name is required.");

        var template = await db.PrintoutTemplates.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Printout template not found.");

        var nameTaken = await db.PrintoutTemplates.AnyAsync(x => x.Name == command.Name && x.Id != command.Id);
        if (nameTaken)
            throw new ConflictException($"A printout template named '{command.Name}' already exists.");

        template.Update(
            command.Name,
            command.FileName ?? template.FileName,
            command.ContentType ?? template.ContentType,
            command.FileContent ?? template.FileContent);
        await db.SaveChangesAsync();
    }
}
