using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Tags.UseCases;

[ApiController]
public sealed class GetTagByIdController : ControllerBase
{
    [HttpGet("/api/tags/{id:guid}")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public Task<GetTagById.Response> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        GetTagById.Execute(id, db);
}

public static class GetTagById
{
    public sealed record Response(Guid Id, string Name, string? ImageUrl, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

    public static async Task<Response> Execute(Guid id, AppDbContext db)
    {
        return await db.Tags
            .Where(x => x.Id == id)
            .Select(x => new Response(x.Id, x.Name, x.ImageUrl, x.CreatedAt, x.UpdatedAt))
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Tag not found.");
    }
}
