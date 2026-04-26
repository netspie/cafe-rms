using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.TaxRates.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.TaxRates.Controllers;

[ApiController]
public sealed class UpdateTaxRateController : ControllerBase
{
    [HttpPut("/api/tax-rates/{id:guid}")]
    [Authorize(Policy = Permissions.TaxRatesManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdateTaxRateRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new UpdateTaxRate.Command(id, request.Name, request.Description, request.Rate);
        await UpdateTaxRate.Execute(command, db);
        return NoContent();
    }
}

public sealed record UpdateTaxRateRequest(string Name, string Description, decimal Rate);

public sealed class UpdateTaxRateValidator : AbstractValidator<UpdateTaxRateRequest>
{
    public UpdateTaxRateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).NotNull().MaximumLength(500);
        RuleFor(x => x.Rate).InclusiveBetween(0m, 100m);
    }
}
