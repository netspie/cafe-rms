using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Tags.UseCases;

[ApiController]
public sealed class UpdateTagController : ControllerBase
{
    [HttpPut("/api/tags/{id:guid}")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdateTag.Request request,
        [FromServices] AppDbContext db)
    {
        await UpdateTag.Execute(id, request, db);
        return NoContent();
    }
}

public static class UpdateTag
{
    public sealed record Request(string Name, string? ImageUrl);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.ImageUrl).MaximumLength(500);
        }
    }

    public static async Task Execute(Guid id, Request request, AppDbContext db)
    {
        var tag = await db.Tags.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Tag not found.");

        var nameTaken = await db.Tags.AnyAsync(x => x.Name == request.Name && x.Id != id);
        if (nameTaken)
            throw new ConflictException($"A tag named '{request.Name}' already exists.");

        tag.Update(request.Name, request.ImageUrl);
        await db.SaveChangesAsync();
    }
}
