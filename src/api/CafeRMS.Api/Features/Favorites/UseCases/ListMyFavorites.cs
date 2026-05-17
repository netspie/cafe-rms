using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Favorites.UseCases;

[ApiController]
public sealed class ListMyFavoritesController : ControllerBase
{
    [HttpGet("/api/my/favorites")]
    [Authorize(Policy = Policies.RequireGuest)]
    public Task<IReadOnlyList<ListMyFavorites.Item>> Handle(
        [FromQuery] string? productName,
        [FromServices] AppDbContext db) =>
        ListMyFavorites.Execute(User.UserId, productName, db);
}


public static class ListMyFavorites
{
    public sealed record Item(Guid ProductId, string ProductName, string? Description);

    public static async Task<IReadOnlyList<Item>> Execute(Guid userId, string? productName, AppDbContext db)
    {
        var queryable = db.Favorites
            .Where(x => x.UserId == userId)
            .Join(
                db.Products,
                f => f.ProductId,
                p => p.Id,
                (f, p) => p);

        if (!string.IsNullOrWhiteSpace(productName))
            queryable = queryable.Where(p => p.Name.Contains(productName));

        return await queryable
            .OrderBy(p => p.Name)
            .Select(p => new Item(p.Id, p.Name, p.Description))
            .ToListAsync();
    }
}
