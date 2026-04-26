using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.PriceGroups.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.PriceGroups.Controllers;

[ApiController]
public sealed class GetPriceGroupByIdController : ControllerBase
{
    [HttpGet("/api/price-groups/{id:guid}")]
    [Authorize(Policy = Permissions.PricingManage)]
    public async Task<GetPriceGroupById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetPriceGroupById.Execute(id, db);
}
