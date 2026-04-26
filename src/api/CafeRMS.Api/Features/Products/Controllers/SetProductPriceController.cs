using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Products.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Products.Controllers;

[ApiController]
public sealed class SetProductPriceController : ControllerBase
{
    [HttpPut("/api/products/{id:guid}/prices/{priceGroupId:guid}")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromRoute] Guid priceGroupId,
        [FromBody] SetProductPriceRequest request,
        [FromServices] AppDbContext db)
    {
        await SetProductPrice.Execute(id, priceGroupId, request.Net, db);
        return NoContent();
    }
}

public sealed record SetProductPriceRequest(decimal Net);

public sealed class SetProductPriceValidator : AbstractValidator<SetProductPriceRequest>
{
    public SetProductPriceValidator()
    {
        RuleFor(x => x.Net).GreaterThanOrEqualTo(0m);
    }
}
