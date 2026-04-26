using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Products.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Products.Controllers;

[ApiController]
public sealed class AddProductController : ControllerBase
{
    [HttpPost("/api/products")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<AddProductResponse> Handle(
        [FromBody] AddProductRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new AddProduct.Command(db.CurrentCompanyId, request.Name, request.Description, request.Barcode, request.TaxRateId);
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
