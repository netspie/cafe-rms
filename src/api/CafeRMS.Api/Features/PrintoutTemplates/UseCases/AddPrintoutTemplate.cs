using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.PrintoutTemplates.UseCases;

[ApiController]
public sealed class AddPrintoutTemplateController : ControllerBase
{
    [HttpPost("/api/printout-templates")]
    [Authorize(Policy = Permissions.PrintoutTemplatesManage)]
    public async Task<AddPrintoutTemplateResponse> Handle(
        [FromForm] string name,
        IFormFile file,
        [FromServices] AppDbContext db)
    {
        var fileContent = await PrintoutFile.ReadAsync(file);
        var command = new AddPrintoutTemplate.Command(name, file.FileName, file.ContentType, fileContent);
        var result = await AddPrintoutTemplate.Execute(command, db);
        return new AddPrintoutTemplateResponse(result.Id);
    }
}

public sealed record AddPrintoutTemplateResponse(Guid Id);


public static class AddPrintoutTemplate
{
    public sealed record Command(string Name, string FileName, string ContentType, byte[] FileContent);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new DomainException("Name is required.");

        var nameTaken = await db.PrintoutTemplates.AnyAsync(x => x.Name == command.Name);
        if (nameTaken)
            throw new ConflictException($"A printout template named '{command.Name}' already exists.");

        var template = PrintoutTemplate.Create(command.Name, command.FileName, command.ContentType, command.FileContent);
        db.PrintoutTemplates.Add(template);
        await db.SaveChangesAsync();
        return new Result(template.Id);
    }
}
