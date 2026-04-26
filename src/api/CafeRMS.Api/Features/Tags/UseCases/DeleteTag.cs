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

        // Phase 2 join-table cleanup: ProductTag rows lose meaning once the Tag is gone.
        var links = await db.ProductTags.Where(x => x.TagId == id).ToListAsync();
        db.ProductTags.RemoveRange(links);

        // SoftDeletableSaveChangesInterceptor converts Remove → soft-delete on the Tag.
        db.Tags.Remove(tag);
        await db.SaveChangesAsync();
    }
}
