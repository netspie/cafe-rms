using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Products.UseCases;

public static class UpdateProduct
{
    public sealed record Command(Guid Id, string Name, string? Description, string? Barcode, Guid TaxRateId);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var product = await db.Products.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Product not found.");

        var taxRateExists = await db.TaxRates.AnyAsync(x => x.Id == command.TaxRateId);
        if (!taxRateExists)
            throw new NotFoundException("Tax rate not found.");

        var nameTaken = await db.Products.AnyAsync(x => x.Name == command.Name && x.Id != command.Id);
        if (nameTaken)
            throw new ConflictException($"A product named '{command.Name}' already exists.");

        if (!string.IsNullOrWhiteSpace(command.Barcode))
        {
            var barcodeTaken = await db.Products.AnyAsync(x => x.Barcode == command.Barcode && x.Id != command.Id);
            if (barcodeTaken)
                throw new ConflictException($"A product with barcode '{command.Barcode}' already exists.");
        }

        product.Update(command.Name, command.Description, command.Barcode, command.TaxRateId);
        await db.SaveChangesAsync();
    }
}
