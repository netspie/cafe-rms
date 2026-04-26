using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.ProductLists.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.ProductLists.Controllers;

[ApiController]
public sealed class AddProductListController : ControllerBase
{
    [HttpPost("/api/product-lists")]
    [Authorize(Policy = Permissions.MenusManage)]
    public async Task<AddProductListResponse> Handle(
        [FromBody] AddProductListRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new AddProductList.Command(db.CurrentCompanyId, request.Name);
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
