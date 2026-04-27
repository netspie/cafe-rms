using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.ProductLists.UseCases;

[ApiController]
public sealed class UpdateProductListController : ControllerBase
{
    [HttpPut("/api/product-lists/{id:guid}")]
    [Authorize(Policy = Permissions.MenusManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdateProductListRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new UpdateProductList.Command(id, request.Name);
        await UpdateProductList.Execute(command, db);
        return NoContent();
    }
}

public sealed record UpdateProductListRequest(string Name);

public sealed class UpdateProductListValidator : AbstractValidator<UpdateProductListRequest>
{
    public UpdateProductListValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}


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
