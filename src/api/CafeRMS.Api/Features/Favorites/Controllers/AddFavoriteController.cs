using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Favorites.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Favorites.Controllers;

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
