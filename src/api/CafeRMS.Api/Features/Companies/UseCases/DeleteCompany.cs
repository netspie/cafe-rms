using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Companies.UseCases;

[ApiController]
public sealed class DeleteCompanyController : ControllerBase
{
    [HttpDelete("/api/companies/{id:guid}")]
    [Authorize(Policy = Policies.RequireSuperAdmin)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await DeleteCompany.Execute(id, db);
        return NoContent();
    }
}


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
