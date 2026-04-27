using CafeRMS.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Favorites.UseCases;

public static class ListMyFavorites
{
    public sealed record Item(Guid ProductId, string ProductName, string? Description);

    public static async Task<IReadOnlyList<Item>> Execute(Guid userId, AppDbContext db)
    {
        // Join through Products with IgnoreQueryFilters since Guests have no company context.
        // Soft-deleted products are filtered out explicitly.
        return await db.Favorites
            .Where(x => x.UserId == userId)
            .Join(
                db.Products.IgnoreQueryFilters().Where(p => p.DeletedAt == null),
                f => f.ProductId,
                p => p.Id,
                (f, p) => new Item(p.Id, p.Name, p.Description))
            .OrderBy(x => x.ProductName)
            .ToListAsync();
    }
}
