using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.ModifierGroups.UseCases;

[ApiController]
public sealed class GetModifierGroupByIdController : ControllerBase
{
    [HttpGet("/api/modifier-groups/{id:guid}")]
    [Authorize]
    public async Task<GetModifierGroupById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetModifierGroupById.Execute(id, db);
}


public static class GetModifierGroupById
{
    public sealed record Result(Guid Id, string Name, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

    public static async Task<Result> Execute(Guid id, AppDbContext db)
    {
        var group = await db.ModifierGroups
            .Where(x => x.Id == id)
            .Select(x => new Result(x.Id, x.Name, x.CreatedAt, x.UpdatedAt))
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Modifier group not found.");

        return group;
    }
}
