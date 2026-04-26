using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Products.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Products.Controllers;

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
