using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.ProductLists.UseCases;

public static class AddProductList
{
    public sealed record Command(Guid CompanyId, string Name);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        if (command.CompanyId == Guid.Empty)
            throw new ForbiddenException("A company context is required to create a product list.");

        var nameTaken = await db.ProductLists.AnyAsync(x => x.Name == command.Name);
        if (nameTaken)
            throw new ConflictException($"A product list named '{command.Name}' already exists.");

        var list = ProductList.Create(command.Name, command.CompanyId);
        db.ProductLists.Add(list);
        await db.SaveChangesAsync();
        return new Result(list.Id);
    }
}
