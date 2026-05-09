using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using CafeRMS.Api.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Favorites.UseCases;

[ApiController]
public sealed class AddFavoriteController : ControllerBase
{
    [HttpPost("/api/my/favorites")]
    [Authorize(Policy = Policies.RequireGuest)]
    public async Task<IActionResult> Handle(
        [FromBody] AddFavoriteRequest request,
        [FromServices] AppDbContext db)
    {
        await AddFavorite.Execute(User.UserId, request.ProductId, db);
        return NoContent();
    }
}

public sealed record AddFavoriteRequest(Guid ProductId);

public sealed class AddFavoriteValidator : AbstractValidator<AddFavoriteRequest>
{
    public AddFavoriteValidator()
    {
        RuleFor(x => x.ProductId).NotEqual(Guid.Empty);
    }
}


public static class AddFavorite
{
    public static async Task Execute(Guid userId, Guid productId, AppDbContext db)
    {
        var productExists = await db.Products.AnyAsync(x => x.Id == productId);
        if (!productExists)
            throw new NotFoundException("Product not found.");

        var alreadyFavorited = await db.Favorites.AnyAsync(x => x.UserId == userId && x.ProductId == productId);
        if (alreadyFavorited)
            return;

        db.Favorites.Add(Favorite.Create(userId, productId));
        await db.SaveChangesAsync();
    }
}
