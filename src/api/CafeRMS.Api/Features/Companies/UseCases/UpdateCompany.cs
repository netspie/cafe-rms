using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Companies.UseCases;

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


public static class UpdateCompany
{
    public sealed record Command(
        Guid Id,
        string LegalName,
        string TaxId,
        string InvoicingAddress,
        string BillingEmail,
        string BillingPhone,
        bool IsPublic);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var company = await db.Companies.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Company not found.");

        var taxIdTaken = await db.Companies.AnyAsync(x => x.TaxId == command.TaxId && x.Id != command.Id);
        if (taxIdTaken)
            throw new ConflictException($"A company with TaxId '{command.TaxId}' already exists.");

        company.Update(
            command.LegalName,
            command.TaxId,
            command.InvoicingAddress,
            command.BillingEmail,
            command.BillingPhone,
            command.IsPublic);

        await db.SaveChangesAsync();
    }
}
