using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Tags.UseCases;

[ApiController]
public sealed class AddTagController : ControllerBase
{
    [HttpPost("/api/tags")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<AddTag.Response> Handle(
        [FromBody] AddTag.Request request,
        [FromServices] AppDbContext db) =>
        await AddTag.Execute(request, db.CurrentCompanyId, db);
}

public static class AddTag
{
    public sealed record Request(string Name, string? ImageUrl);
    public sealed record Response(Guid Id);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.ImageUrl).MaximumLength(500);
        }
    }

    public static async Task<Response> Execute(Request request, Guid companyId, AppDbContext db)
    {
        if (companyId == Guid.Empty)
            throw new ForbiddenException("A company context is required to create a tag.");

        var nameTaken = await db.Tags.AnyAsync(x => x.Name == request.Name);
        if (nameTaken)
            throw new ConflictException($"A tag named '{request.Name}' already exists.");

        var tag = Tag.Create(request.Name, companyId, request.ImageUrl);
        db.Tags.Add(tag);
        await db.SaveChangesAsync();
        return new Response(tag.Id);
    }
}
