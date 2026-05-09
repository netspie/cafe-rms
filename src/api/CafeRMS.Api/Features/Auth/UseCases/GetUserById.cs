using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Auth.UseCases;

[ApiController]
public sealed class GetUserByIdController : ControllerBase
{
    [HttpGet("/api/users/{id:guid}")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<GetUserById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetUserById.Execute(id, db);
}


public static class GetUserById
{
    public sealed record Result(
        Guid Id,
        string Email,
        string FirstName,
        string LastName,
        IReadOnlyList<string> Roles);

    public static async Task<Result> Execute(Guid userId, AppDbContext db)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId)
            ?? throw new NotFoundException("User not found.");

        if (user.AccountType != AccountType.Staff)
            throw new ForbiddenException("Only staff users are accessible via this endpoint.");

        var roles = await db.UserRoles
            .Where(x => x.UserId == user.Id)
            .Join(db.Roles, x => x.RoleId, x => x.Id, (x, y) => y.Name ?? "")
            .ToListAsync();

        return new Result(user.Id, user.Email ?? "", user.FirstName, user.LastName, roles);
    }
}
