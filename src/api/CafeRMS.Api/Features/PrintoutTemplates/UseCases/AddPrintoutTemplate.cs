using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.PrintoutTemplates.UseCases;

public static class AddPrintoutTemplate
{
    public sealed record Command(Guid CompanyId, string Name, string TemplateFileUrl);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        if (command.CompanyId == Guid.Empty)
            throw new ForbiddenException("A company context is required to create a printout template.");

        var nameTaken = await db.PrintoutTemplates.AnyAsync(x => x.Name == command.Name);
        if (nameTaken)
            throw new ConflictException($"A printout template named '{command.Name}' already exists.");

        var template = PrintoutTemplate.Create(command.Name, command.TemplateFileUrl, command.CompanyId);
        db.PrintoutTemplates.Add(template);
        await db.SaveChangesAsync();
        return new Result(template.Id);
    }
}
