using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.TaxRates.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.TaxRates.Controllers;

[ApiController]
public sealed class AddTaxRateController : ControllerBase
{
    [HttpPost("/api/tax-rates")]
    [Authorize(Policy = Permissions.TaxRatesManage)]
    public async Task<AddTaxRateResponse> Handle(
        [FromBody] AddTaxRateRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new AddTaxRate.Command(db.CurrentCompanyId, request.Name, request.Description, request.Rate);
        var result = await AddTaxRate.Execute(command, db);
        return new AddTaxRateResponse(result.Id);
    }
}

public sealed record AddTaxRateRequest(string Name, string Description, decimal Rate);

public sealed record AddTaxRateResponse(Guid Id);

public sealed class AddTaxRateValidator : AbstractValidator<AddTaxRateRequest>
{
    public AddTaxRateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).NotNull().MaximumLength(500);
        RuleFor(x => x.Rate).InclusiveBetween(0m, 100m);
    }
}
