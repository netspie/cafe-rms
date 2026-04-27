using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Orders.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Orders.Controllers;

[ApiController]
public sealed class CancelMyOrderController : ControllerBase
{
    [HttpPost("/api/my/orders/{id:guid}/cancel")]
    [Authorize(Policy = Policies.RequireGuest)]
    [ResourceOwner<Order>("id", nameof(Order.UserId))]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] CancelMyOrderRequest request,
        [FromServices] AppDbContext db)
    {
        await CancelOrder.Execute(id, request.Reason, calledByCustomer: true, db, DateTimeOffset.UtcNow);
        return NoContent();
    }
}

public sealed record CancelMyOrderRequest(string? Reason);

public sealed class CancelMyOrderValidator : AbstractValidator<CancelMyOrderRequest>
{
    public CancelMyOrderValidator()
    {
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
