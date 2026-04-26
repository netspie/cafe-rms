using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Products.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Products.Controllers;

[ApiController]
public sealed class AddProductImageController : ControllerBase
{
    [HttpPost("/api/products/{id:guid}/images")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<AddProductImageResponse> Handle(
        [FromRoute] Guid id,
        [FromBody] AddProductImageRequest request,
        [FromServices] AppDbContext db)
    {
        var result = await AddProductImage.Execute(id, request.Url, db);
        return new AddProductImageResponse(result.Id);
    }
}

public sealed record AddProductImageRequest(string Url);

public sealed record AddProductImageResponse(Guid Id);

public sealed class AddProductImageValidator : AbstractValidator<AddProductImageRequest>
{
    public AddProductImageValidator()
    {
        RuleFor(x => x.Url).NotEmpty().MaximumLength(500);
    }
}
