using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Products.UseCases;

[ApiController]
public sealed class UpdateProductController : ControllerBase
{
    [HttpPut("/api/products/{id:guid}")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdateProductRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new UpdateProduct.Command(id, request.Name, request.Description, request.Barcode, request.TaxRateId);
        await UpdateProduct.Execute(command, db);
        return NoContent();
    }
}

public sealed record UpdateProductRequest(string Name, string? Description, string? Barcode, Guid TaxRateId);

public sealed class UpdateProductValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Barcode).MaximumLength(100);
        RuleFor(x => x.TaxRateId).NotEqual(Guid.Empty);
    }
}


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
