using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.ProductLists.UseCases;

[ApiController]
public sealed class AddProductListController : ControllerBase
{
    [HttpPost("/api/product-lists")]
    [Authorize(Policy = Permissions.MenusManage)]
    public async Task<AddProductListResponse> Handle(
        [FromBody] AddProductListRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new AddProductList.Command(request.Name);
        var result = await AddProductList.Execute(command, db);
        return new AddProductListResponse(result.Id);
    }
}

public sealed record AddProductListRequest(string Name);

public sealed record AddProductListResponse(Guid Id);

public sealed class AddProductListValidator : AbstractValidator<AddProductListRequest>
{
    public AddProductListValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}


public static class AddProductList
{
    public sealed record Command(string Name);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        var nameTaken = await db.ProductLists.AnyAsync(x => x.Name == command.Name);
        if (nameTaken)
            throw new ConflictException($"A product list named '{command.Name}' already exists.");

        var list = ProductList.Create(command.Name);
        db.ProductLists.Add(list);
        await db.SaveChangesAsync();
        return new Result(list.Id);
    }
}
