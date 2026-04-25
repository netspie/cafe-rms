using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Companies.UseCases;

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
