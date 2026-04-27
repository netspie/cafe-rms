using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Orders.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Orders.Controllers;

[ApiController]
public sealed class PlaceWalkInOrderController : ControllerBase
{
    [HttpPost("/api/orders")]
    [Authorize(Policy = Permissions.OrdersManage)]
    public async Task<PlaceWalkInOrderResponse> Handle(
        [FromBody] PlaceWalkInOrderRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new PlaceOrder.Command(
            request.OutletId,
            CallerCompanyId: db.CurrentCompanyId, // Staff — used to reject cross-tenant outlet ids.
            request.UserId,
            request.TableId,
            request.SalesChannelId,
            request.EventId,
            request.PromotionCode,
            request.LoyaltyPointsUsed,
            request.Lines.Select(x => new PlaceOrder.LineInput(x.ProductId, x.Quantity, x.PriceGroupId)).ToList());

        var result = await PlaceOrder.Execute(command, db, DateTimeOffset.UtcNow);
        return new PlaceWalkInOrderResponse(result.OrderId);
    }
}

public sealed record PlaceWalkInOrderRequest(
    Guid OutletId,
    Guid? UserId,
    Guid? TableId,
    Guid? SalesChannelId,
    Guid? EventId,
    string? PromotionCode,
    int LoyaltyPointsUsed,
    IReadOnlyList<PlaceOrderLineRequest> Lines);

public sealed record PlaceWalkInOrderResponse(Guid OrderId);

public sealed class PlaceWalkInOrderRequestValidator : AbstractValidator<PlaceWalkInOrderRequest>
{
    public PlaceWalkInOrderRequestValidator()
    {
        RuleFor(x => x.OutletId).NotEqual(Guid.Empty);
        RuleFor(x => x.LoyaltyPointsUsed).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).NotEqual(Guid.Empty);
            line.RuleFor(l => l.Quantity).GreaterThan(0);
        });
    }
}
