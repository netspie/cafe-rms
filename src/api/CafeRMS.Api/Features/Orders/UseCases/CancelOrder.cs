using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Orders.UseCases;

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


public static class CancelOrder
{
    // calledByCustomer = true → only allowed before AcceptedAt (customer can't take back
    // an order kitchen has already accepted).
    public static async Task Execute(Guid id, string? reason, bool calledByCustomer, AppDbContext db, DateTimeOffset now)
    {
        var order = await db.Orders.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Order not found.");

        if (order.IsCancelled)
            throw new ConflictException("Order is already cancelled.");
        if (order.IsClosed)
            throw new ConflictException("Cannot cancel a closed order.");
        if (calledByCustomer && order.AcceptedAt is not null)
            throw new ForbiddenException("Order can no longer be cancelled by the customer — it has been accepted.");

        order.Cancel(now, reason);
        await db.SaveChangesAsync();
    }
}
