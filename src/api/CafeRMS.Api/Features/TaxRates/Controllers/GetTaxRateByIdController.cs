using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.TaxRates.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.TaxRates.Controllers;

[ApiController]
public sealed class GetTaxRateByIdController : ControllerBase
{
    [HttpGet("/api/tax-rates/{id:guid}")]
    [Authorize(Policy = Permissions.TaxRatesManage)]
    public async Task<GetTaxRateById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetTaxRateById.Execute(id, db);
}
