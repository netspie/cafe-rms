using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.ProductLists.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.ProductLists.Controllers;

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
