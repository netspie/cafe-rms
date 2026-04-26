using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Tags.UseCases;

public static class DeleteTag
{
    public static async Task Execute(Guid id, AppDbContext db)
    {
        var tag = await db.Tags.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Tag not found.");

        // SoftDeletableSaveChangesInterceptor converts Remove → soft-delete.
        db.Tags.Remove(tag);
        await db.SaveChangesAsync();
    }
}
