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
    // Cafe-simple lifecycle: any non-terminal order can be cancelled by either side.
    // The `calledByCustomer` parameter is kept on the signature for symmetry with the
    // staff/guest controllers but doesn't gate anything anymore — there are no
    // intermediate states between Placed and Closed where staff would want to lock the
    // customer out.
    public static async Task Execute(Guid id, string? reason, bool calledByCustomer, AppDbContext db, DateTimeOffset now)
    {
        _ = calledByCustomer;
        var order = await db.Orders.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Order not found.");

        if (order.IsCancelled)
            throw new ConflictException("Order is already cancelled.");
        if (order.IsClosed)
            throw new ConflictException("Cannot cancel a closed order.");

        order.Cancel(now, reason);
        await db.SaveChangesAsync();
    }
}
