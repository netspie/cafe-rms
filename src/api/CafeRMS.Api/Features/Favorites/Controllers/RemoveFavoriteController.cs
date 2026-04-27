using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Favorites.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Favorites.Controllers;

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
