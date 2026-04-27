using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Favorites.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Favorites.Controllers;

[ApiController]
public sealed class ListMyFavoritesController : ControllerBase
{
    [HttpGet("/api/my/favorites")]
    [Authorize(Policy = Policies.RequireGuest)]
    public Task<IReadOnlyList<ListMyFavorites.Item>> Handle(
        [FromServices] AppDbContext db) =>
        ListMyFavorites.Execute(User.UserId, db);
}
