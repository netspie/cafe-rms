using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.TaxRates.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.TaxRates.Controllers;

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
