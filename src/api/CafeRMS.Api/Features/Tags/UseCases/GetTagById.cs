using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Tags.UseCases;

public static class GetTagById
{
    public sealed record Result(Guid Id, string Name, string? ImageUrl, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

    public static async Task<Result> Execute(Guid id, AppDbContext db)
    {
        var tag = await db.Tags
            .Where(x => x.Id == id)
            .Select(x => new Result(x.Id, x.Name, x.ImageUrl, x.CreatedAt, x.UpdatedAt))
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Tag not found.");

        return tag;
    }
}
