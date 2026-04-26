using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Companies.UseCases;

public static class DeleteCompany
{
    public static async Task Execute(Guid id, AppDbContext db)
    {
        var company = await db.Companies.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Company not found.");

        // SoftDeletableSaveChangesInterceptor converts the Remove into a soft-delete:
        // sets DeletedAt + DeletedBy, leaves the row in place. Outlets, roles, users
        // referencing this company stay intact (they'll be filtered out by query
        // filters once the parent company disappears from filtered queries).
        db.Companies.Remove(company);
        await db.SaveChangesAsync();
    }
}
