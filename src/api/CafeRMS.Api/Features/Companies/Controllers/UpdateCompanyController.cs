using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Companies.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Companies.Controllers;

[ApiController]
public sealed class UpdateCompanyController : ControllerBase
{
    [HttpPut("/api/companies/{id:guid}")]
    [Authorize(Policy = Policies.RequireSuperAdmin)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdateCompanyRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new UpdateCompany.Command(
            id,
            request.LegalName,
            request.TaxId,
            request.InvoicingAddress,
            request.BillingEmail,
            request.BillingPhone,
            request.IsPublic);

        await UpdateCompany.Execute(command, db);
        return NoContent();
    }
}

public sealed record UpdateCompanyRequest(
    string LegalName,
    string TaxId,
    string InvoicingAddress,
    string BillingEmail,
    string BillingPhone,
    bool IsPublic);

public sealed class UpdateCompanyValidator : AbstractValidator<UpdateCompanyRequest>
{
    public UpdateCompanyValidator()
    {
        RuleFor(x => x.LegalName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TaxId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.InvoicingAddress).NotEmpty().MaximumLength(500);
        RuleFor(x => x.BillingEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.BillingPhone).NotEmpty().MaximumLength(50);
    }
}
