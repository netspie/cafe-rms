using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Allergens.UseCases;

[ApiController]
public sealed class DeleteAllergenController : ControllerBase
{
    [HttpDelete("/api/allergens/{id:guid}")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await DeleteAllergen.Execute(id, db);
        return NoContent();
    }
}

public static class DeleteAllergen
{
    public static async Task Execute(Guid id, AppDbContext db)
    {
        var allergen = await db.Allergens.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Allergen not found.");

        var links = await db.ProductAllergens.Where(x => x.AllergenId == id).ToListAsync();
        db.ProductAllergens.RemoveRange(links);

        db.Allergens.Remove(allergen);
        await db.SaveChangesAsync();
    }
}
