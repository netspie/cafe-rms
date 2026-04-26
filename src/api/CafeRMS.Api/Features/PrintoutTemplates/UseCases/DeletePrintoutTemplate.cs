using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.PrintoutTemplates.UseCases;

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
