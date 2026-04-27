using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.TaxRates.UseCases;

[ApiController]
public sealed class DeleteTaxRateController : ControllerBase
{
    [HttpDelete("/api/tax-rates/{id:guid}")]
    [Authorize(Policy = Permissions.TaxRatesManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await DeleteTaxRate.Execute(id, db);
        return NoContent();
    }
}


public static class DeleteTaxRate
{
    public static async Task Execute(Guid id, AppDbContext db)
    {
        var taxRate = await db.TaxRates.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Tax rate not found.");

        db.TaxRates.Remove(taxRate);
        await db.SaveChangesAsync();
    }
}
