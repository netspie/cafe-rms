using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Products.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Products.Controllers;

[ApiController]
public sealed class GetProductByIdController : ControllerBase
{
    [HttpGet("/api/products/{id:guid}")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<GetProductById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetProductById.Execute(id, db);
}
