using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.PrintoutTemplates.UseCases;

public static class UpdatePrintoutTemplate
{
    public sealed record Command(Guid Id, string Name, string TemplateFileUrl);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var template = await db.PrintoutTemplates.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Printout template not found.");

        var nameTaken = await db.PrintoutTemplates.AnyAsync(x => x.Name == command.Name && x.Id != command.Id);
        if (nameTaken)
            throw new ConflictException($"A printout template named '{command.Name}' already exists.");

        template.Update(command.Name, command.TemplateFileUrl);
        await db.SaveChangesAsync();
    }
}
