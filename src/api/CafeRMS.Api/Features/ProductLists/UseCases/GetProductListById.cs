using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.ProductLists.UseCases;

[ApiController]
public sealed class GetProductListByIdController : ControllerBase
{
    [HttpGet("/api/product-lists/{id:guid}")]
    [Authorize(Policy = Permissions.MenusManage)]
    public async Task<GetProductListById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetProductListById.Execute(id, db);
}


public static class GetProductListById
{
    public sealed record Result(
        Guid Id,
        string Name,
        IReadOnlyList<Guid> ProductIds,
        DateTimeOffset CreatedAt,
        DateTimeOffset? UpdatedAt);

    public static async Task<Result> Execute(Guid id, AppDbContext db)
    {
        var list = await db.ProductLists.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Product list not found.");

        var productIds = await db.ProductListItems
            .Where(x => x.ProductListId == id)
            .Select(x => x.ProductId)
            .ToListAsync();

        return new Result(list.Id, list.Name, productIds, list.CreatedAt, list.UpdatedAt);
    }
}
