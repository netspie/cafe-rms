using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Outlets.UseCases;

public static class UpdateOutlet
{
    public sealed record Command(
        Guid Id,
        string DisplayName,
        string StreetAddress,
        string Phone,
        string TimeZone,
        Currency Currency,
        string? LogoUrl);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var outlet = await db.Outlets.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Outlet not found.");

        outlet.Update(command.DisplayName, command.StreetAddress, command.Phone, command.TimeZone, command.Currency, command.LogoUrl);
        await db.SaveChangesAsync();
    }
}
