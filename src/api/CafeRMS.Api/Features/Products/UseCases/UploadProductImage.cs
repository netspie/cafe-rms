using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Products.UseCases;

[ApiController]
public sealed class UploadProductImageController : ControllerBase
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxBytes = 5 * 1024 * 1024;

    [HttpPost("/api/products/{id:guid}/images/upload")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<AddProductImageResponse> Handle(
        [FromRoute] Guid id,
        IFormFile file,
        [FromServices] AppDbContext db,
        [FromServices] IWebHostEnvironment env)
    {
        if (file is null || file.Length == 0)
            throw new DomainException("No file was uploaded.");
        if (file.Length > MaxBytes)
            throw new DomainException("Image is too large (max 5 MB).");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new DomainException("Unsupported image type. Use JPG, PNG or WebP.");

        var productExists = await db.Products.AnyAsync(x => x.Id == id);
        if (!productExists)
            throw new NotFoundException("Product not found.");

        var uploadsPath = Path.Combine(env.ContentRootPath, "uploads");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}{extension}";
        await using (var stream = System.IO.File.Create(Path.Combine(uploadsPath, fileName)))
            await file.CopyToAsync(stream);

        var image = ProductImage.Create(id, $"/uploads/{fileName}");
        db.ProductImages.Add(image);
        await db.SaveChangesAsync();

        return new AddProductImageResponse(image.Id);
    }
}
