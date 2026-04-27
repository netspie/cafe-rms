using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Orders.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Orders.Controllers;

[ApiController]
public sealed class PlaceMyOrderController : ControllerBase
{
    [HttpPost("/api/my/orders")]
    [Authorize(Policy = Policies.RequireGuest)]
    public async Task<PlaceOrderResponse> Handle(
        [FromBody] PlaceOrderRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new PlaceOrder.Command(
            request.OutletId,
            CallerCompanyId: null, // Guest — company is derived from the outlet inside the use case.
            User.UserId,
            request.TableId,
            request.SalesChannelId,
            request.EventId,
            request.PromotionCode,
            request.LoyaltyPointsUsed,
            request.Lines.Select(x => new PlaceOrder.LineInput(x.ProductId, x.Quantity, x.PriceGroupId)).ToList());

        var result = await PlaceOrder.Execute(command, db, DateTimeOffset.UtcNow);
        return new PlaceOrderResponse(result.OrderId);
    }
}

public sealed record PlaceOrderRequest(
    Guid OutletId,
    Guid? TableId,
    Guid? SalesChannelId,
    Guid? EventId,
    string? PromotionCode,
    int LoyaltyPointsUsed,
    IReadOnlyList<PlaceOrderLineRequest> Lines);

public sealed record PlaceOrderLineRequest(Guid ProductId, int Quantity, Guid? PriceGroupId);

public sealed record PlaceOrderResponse(Guid OrderId);

public sealed class PlaceOrderRequestValidator : AbstractValidator<PlaceOrderRequest>
{
    public PlaceOrderRequestValidator()
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
