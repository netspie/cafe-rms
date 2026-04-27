using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Orders.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Orders.Controllers;

[ApiController]
public sealed class GetMyOrderByIdController : ControllerBase
{
    [HttpGet("/api/my/orders/{id:guid}")]
    [Authorize(Policy = Policies.RequireGuest)]
    [ResourceOwner<Order>("id", nameof(Order.UserId))]
    public async Task<GetOrderById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetOrderById.Execute(id, db);
}
