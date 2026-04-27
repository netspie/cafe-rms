using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Products.UseCases;

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


public static class AddProductImage
{
    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Guid productId, string url, AppDbContext db)
    {
        var productExists = await db.Products.AnyAsync(x => x.Id == productId);
        if (!productExists)
            throw new NotFoundException("Product not found.");

        var image = ProductImage.Create(productId, url);
        db.ProductImages.Add(image);
        await db.SaveChangesAsync();
        return new Result(image.Id);
    }
}
