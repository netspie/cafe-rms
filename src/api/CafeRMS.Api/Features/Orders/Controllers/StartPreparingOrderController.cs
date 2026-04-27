using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Orders.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Orders.Controllers;

[ApiController]
public sealed class StartPreparingOrderController : ControllerBase
{
    [HttpPost("/api/orders/{id:guid}/start-preparing")]
    [Authorize(Policy = Permissions.OrdersManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await StartPreparingOrder.Execute(id, db, DateTimeOffset.UtcNow);
        return NoContent();
    }
}
