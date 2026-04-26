using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.PromotionCodes.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.PromotionCodes.Controllers;

[ApiController]
public sealed class GetPromotionCodeByIdController : ControllerBase
{
    [HttpGet("/api/promotion-codes/{id:guid}")]
    [Authorize(Policy = Permissions.PromotionsManage)]
    public async Task<GetPromotionCodeById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetPromotionCodeById.Execute(id, db);
}
