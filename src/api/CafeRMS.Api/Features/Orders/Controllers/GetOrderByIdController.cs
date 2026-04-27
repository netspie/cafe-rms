using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Orders.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Orders.Controllers;

[ApiController]
public sealed class GetOrderByIdController : ControllerBase
{
    [HttpGet("/api/orders/{id:guid}")]
    [Authorize(Policy = Permissions.OrdersView)]
    public async Task<GetOrderById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetOrderById.Execute(id, db);
}
