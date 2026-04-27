using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using CafeRMS.Api.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Favorites.UseCases;

[ApiController]
public sealed class RemoveFavoriteController : ControllerBase
{
    [HttpDelete("/api/my/favorites/{productId:guid}")]
    [Authorize(Policy = Policies.RequireGuest)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid productId,
        [FromServices] AppDbContext db)
    {
        await RemoveFavorite.Execute(User.UserId, productId, db);
        return NoContent();
    }
}


public static class RemoveFavorite
{
    public static async Task Execute(Guid userId, Guid productId, AppDbContext db)
    {
        var favorite = await db.Favorites.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId)
            ?? throw new NotFoundException("Favorite not found.");

        db.Favorites.Remove(favorite);
        await db.SaveChangesAsync();
    }
}
