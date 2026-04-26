using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.ProductLists.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.ProductLists.Controllers;

[ApiController]
public sealed class AddProductToListController : ControllerBase
{
    [HttpPost("/api/product-lists/{id:guid}/items")]
    [Authorize(Policy = Permissions.MenusManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] AddProductToListRequest request,
        [FromServices] AppDbContext db)
    {
        await AddProductToList.Execute(id, request.ProductId, db);
        return NoContent();
    }
}

public sealed record AddProductToListRequest(Guid ProductId);

public sealed class AddProductToListValidator : AbstractValidator<AddProductToListRequest>
{
    public AddProductToListValidator()
    {
        RuleFor(x => x.ProductId).NotEqual(Guid.Empty);
    }
}
