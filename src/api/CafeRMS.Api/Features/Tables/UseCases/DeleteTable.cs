using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Tables.UseCases;

public static class DeleteTable
{
    public static async Task Execute(Guid id, AppDbContext db)
    {
        var table = await db.Tables.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Table not found.");

        db.Tables.Remove(table);
        await db.SaveChangesAsync();
    }
}
