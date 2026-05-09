using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Products.UseCases;

[ApiController]
public sealed class AddProductController : ControllerBase
{
    [HttpPost("/api/products")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<AddProductResponse> Handle(
        [FromBody] AddProductRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new AddProduct.Command(request.Name, request.Description, request.Barcode, request.TaxRateId);
        var result = await AddProduct.Execute(command, db);
        return new AddProductResponse(result.Id);
    }
}

public sealed record AddProductRequest(string Name, string? Description, string? Barcode, Guid TaxRateId);

public sealed record AddProductResponse(Guid Id);

public sealed class AddProductValidator : AbstractValidator<AddProductRequest>
{
    public AddProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Barcode).MaximumLength(100);
        RuleFor(x => x.TaxRateId).NotEqual(Guid.Empty);
    }
}


public static class AddProduct
{
    public sealed record Command(string Name, string? Description, string? Barcode, Guid TaxRateId);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
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

        var product = Product.Create(command.Name, command.TaxRateId, command.Description, command.Barcode);
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return new Result(product.Id);
    }
}
