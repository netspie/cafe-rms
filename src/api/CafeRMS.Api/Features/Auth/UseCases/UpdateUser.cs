using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Auth.UseCases;

public static class UpdateUser
{
    public sealed record Command(Guid CompanyId, Guid UserId, string FirstName, string LastName);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == command.UserId)
            ?? throw new NotFoundException("User not found.");

        if (user.AccountType != AccountType.Staff || user.CompanyId != command.CompanyId)
            throw new ForbiddenException("User is not a staff member of the current company.");

        user.UpdateProfile(command.FirstName, command.LastName);
        await db.SaveChangesAsync();
    }
}
