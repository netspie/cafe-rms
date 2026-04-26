using CafeRMS.Api.Features.Allergens.UseCases;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Allergens.Controllers;

[ApiController]
public sealed class GetAllergenByIdController : ControllerBase
{
    [HttpGet("/api/allergens/{id:guid}")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<GetAllergenById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetAllergenById.Execute(id, db);
}
