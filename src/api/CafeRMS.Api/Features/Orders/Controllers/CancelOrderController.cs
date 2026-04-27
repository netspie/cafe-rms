using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Orders.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Orders.Controllers;

[ApiController]
public sealed class CancelOrderController : ControllerBase
{
    [HttpPost("/api/orders/{id:guid}/cancel")]
    [Authorize(Policy = Permissions.OrdersManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] CancelOrderRequest request,
        [FromServices] AppDbContext db)
    {
        await CancelOrder.Execute(id, request.Reason, calledByCustomer: false, db, DateTimeOffset.UtcNow);
        return NoContent();
    }
}

public sealed record CancelOrderRequest(string? Reason);

public sealed class CancelOrderValidator : AbstractValidator<CancelOrderRequest>
{
    public CancelOrderValidator()
    {
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
