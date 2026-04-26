using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.ProductLists.UseCases;

public static class UpdateProductList
{
    public sealed record Command(Guid Id, string Name);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var list = await db.ProductLists.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Product list not found.");

        var nameTaken = await db.ProductLists.AnyAsync(x => x.Name == command.Name && x.Id != command.Id);
        if (nameTaken)
            throw new ConflictException($"A product list named '{command.Name}' already exists.");

        list.Update(command.Name);
        await db.SaveChangesAsync();
    }
}
