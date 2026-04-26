using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Tags.UseCases;

public static class UpdateTag
{
    public sealed record Command(Guid Id, string Name, string? ImageUrl);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var tag = await db.Tags.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Tag not found.");

        var nameTaken = await db.Tags.AnyAsync(x => x.Name == command.Name && x.Id != command.Id);
        if (nameTaken)
            throw new ConflictException($"A tag named '{command.Name}' already exists.");

        tag.Update(command.Name, command.ImageUrl);
        await db.SaveChangesAsync();
    }
}
