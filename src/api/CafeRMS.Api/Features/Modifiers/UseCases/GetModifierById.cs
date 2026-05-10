using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Modifiers.UseCases;

[ApiController]
public sealed class GetModifierByIdController : ControllerBase
{
    [HttpGet("/api/modifiers/{id:guid}")]
    [Authorize]
    public async Task<GetModifierById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetModifierById.Execute(id, db);
}


public static class GetModifierById
{
    public sealed record Result(Guid Id, string Name, Guid ModifierGroupId, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

    public static async Task<Result> Execute(Guid id, AppDbContext db)
    {
        var modifier = await db.Modifiers
            .Where(x => x.Id == id)
            .Select(x => new Result(x.Id, x.Name, x.ModifierGroupId, x.CreatedAt, x.UpdatedAt))
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Modifier not found.");

        return modifier;
    }
}
