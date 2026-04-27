using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Loyalty.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Loyalty.Controllers;

[ApiController]
public sealed class GetMyLoyaltyBalanceController : ControllerBase
{
    [HttpGet("/api/my/loyalty/balance")]
    [Authorize(Policy = Policies.RequireGuest)]
    public Task<GetMyLoyaltyBalance.Result> Handle(
        [FromServices] AppDbContext db) =>
        GetMyLoyaltyBalance.Execute(User.UserId, db);
}
