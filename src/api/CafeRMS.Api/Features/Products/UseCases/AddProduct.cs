using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Products.UseCases;

public static class AddProduct
{
    public sealed record Command(Guid CompanyId, string Name, string? Description, string? Barcode, Guid TaxRateId);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        if (command.CompanyId == Guid.Empty)
            throw new ForbiddenException("A company context is required to create a product.");

        var taxRateExists = await db.TaxRates.AnyAsync(x => x.Id == command.TaxRateId);
        if (!taxRateExists)
            throw new NotFoundException("Tax rate not found.");

        var nameTaken = await db.Products.AnyAsync(x => x.Name == command.Name);
        if (nameTaken)
            throw new ConflictException($"A product named '{command.Name}' already exists.");

        if (!string.IsNullOrWhiteSpace(command.Barcode))
        {
            var barcodeTaken = await db.Products.AnyAsync(x => x.Barcode == command.Barcode);
            if (barcodeTaken)
                throw new ConflictException($"A product with barcode '{command.Barcode}' already exists.");
        }

        var product = Product.Create(command.Name, command.TaxRateId, command.CompanyId, command.Description, command.Barcode);
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return new Result(product.Id);
    }
}
