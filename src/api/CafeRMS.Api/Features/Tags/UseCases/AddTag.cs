using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Tags.UseCases;

public static class AddTag
{
    public sealed record Command(Guid CompanyId, string Name, string? ImageUrl);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        if (command.CompanyId == Guid.Empty)
            throw new ForbiddenException("A company context is required to create a tag.");

        var nameTaken = await db.Tags.AnyAsync(x => x.Name == command.Name);
        if (nameTaken)
            throw new ConflictException($"A tag named '{command.Name}' already exists.");

        var tag = Tag.Create(command.Name, command.CompanyId, command.ImageUrl);
        db.Tags.Add(tag);
        await db.SaveChangesAsync();
        return new Result(tag.Id);
    }
}
