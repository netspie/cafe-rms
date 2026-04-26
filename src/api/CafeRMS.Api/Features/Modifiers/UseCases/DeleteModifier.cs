using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Modifiers.UseCases;

public static class DeleteModifier
{
    public static async Task Execute(Guid id, AppDbContext db)
    {
        var modifier = await db.Modifiers.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Modifier not found.");

        db.Modifiers.Remove(modifier);
        await db.SaveChangesAsync();
    }
}
