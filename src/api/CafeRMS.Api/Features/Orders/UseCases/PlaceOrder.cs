using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Events;
using CafeRMS.Api.Features.Loyalty;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Orders.UseCases;

[ApiController]
public sealed class PlaceMyOrderController : ControllerBase
{
    [HttpPost("/api/my/orders")]
    [Authorize(Policy = Policies.RequireGuest)]
    public async Task<PlaceOrderResponse> Handle(
        [FromBody] PlaceOrderRequest request,
        [FromServices] AppDbContext db)
    {
        var result = await PlaceOrder.Execute(request, User.UserId, db, DateTimeOffset.UtcNow);
        return new PlaceOrderResponse(result.OrderId);
    }
}

public sealed record PlaceOrderRequest(
    Guid OutletId,
    Guid? TableId,
    Guid? SalesChannelId,
    string? PromotionCode,
    int LoyaltyPointsUsed,
    IReadOnlyList<PlaceOrderLineRequest> Lines);

public sealed record PlaceOrderLineRequest(Guid ProductId, int Quantity, IReadOnlyList<Guid>? ModifierIds = null);

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

public static class PlaceOrder
{
    public sealed record Result(Guid OrderId);

    public static async Task<Result> Execute(PlaceOrderRequest request, Guid userId, AppDbContext db, DateTimeOffset now)
    {
        if (request.Lines.Count == 0)
            throw new DomainException("Order must have at least one line.");

        var outlet = await db.Outlets.FirstOrDefaultAsync(x => x.Id == request.OutletId)
            ?? throw new NotFoundException("Outlet not found.");

        if (request.TableId is Guid tableId)
        {
            var tableInOutlet = await db.Tables.AnyAsync(x => x.Id == tableId && x.OutletId == request.OutletId);
            if (!tableInOutlet)
                throw new NotFoundException("Table not found.");
        }

        if (request.SalesChannelId is Guid salesChannelId)
        {
            var salesChannel = await db.SalesChannels.FirstOrDefaultAsync(x => x.Id == salesChannelId)
                ?? throw new NotFoundException("Sales channel not found.");
            if (!salesChannel.IsTakeout && request.TableId is null)
                throw new DomainException("Dine-in orders require a table.");
        }

        var activeEvent = await ActiveEventResolver.ResolveAsync(db, now);
        var eventProductIds = (activeEvent?.ProductIds ?? new List<Guid>()).ToHashSet();
        var orderEventId = activeEvent is not null && request.Lines.Any(l => eventProductIds.Contains(l.ProductId))
            ? activeEvent.Id
            : (Guid?)null;

        await using var tx = await db.Database.BeginTransactionAsync();

        var order = Order.Create(
            request.OutletId,
            request.TableId,
            request.SalesChannelId,
            userId,
            orderEventId);

        var (lines, subtotal) = await BuildLinesAsync(
            request.Lines, order.Id, outlet.DefaultPriceGroupId, activeEvent?.PriceGroupId, eventProductIds, db);

        order.RedeemLoyaltyPoints(request.LoyaltyPointsUsed, subtotal);

        if (!string.IsNullOrWhiteSpace(request.PromotionCode))
            await ApplyPromotionAsync(request.PromotionCode, subtotal, order, now, db);

        if (request.LoyaltyPointsUsed > 0)
            await RedeemLoyaltyPointsAsync(userId, request.LoyaltyPointsUsed, order.Id, db);

        db.Orders.Add(order);
        db.OrderLines.AddRange(lines);
        await db.SaveChangesAsync();
        await tx.CommitAsync();

        return new Result(order.Id);
    }

    private static async Task<(List<OrderLine> Lines, decimal Subtotal)> BuildLinesAsync(
        IReadOnlyList<PlaceOrderLineRequest> requestLines,
        Guid orderId,
        Guid? defaultPriceGroupId,
        Guid? eventPriceGroupId,
        HashSet<Guid> eventProductIds,
        AppDbContext db)
    {
        var lines = new List<OrderLine>();
        decimal subtotal = 0m;

        foreach (var line in requestLines)
        {
            if (line.Quantity <= 0)
                throw new DomainException("Quantity must be positive.");

            var product = await db.Products
                .Include(x => x.TaxRate)
                .FirstOrDefaultAsync(x => x.Id == line.ProductId)
                ?? throw new NotFoundException($"Product {line.ProductId} not found.");

            var priceGroupId = eventProductIds.Contains(line.ProductId)
                ? eventPriceGroupId
                : defaultPriceGroupId;

            var priceQuery = db.ProductPrices.Where(x => x.ProductId == line.ProductId);
            if (priceGroupId is Guid pgId)
                priceQuery = priceQuery.Where(x => x.PriceGroupId == pgId);
                
            var price = await priceQuery.FirstOrDefaultAsync()
                ?? throw new NotFoundException($"No price set for product {line.ProductId}.");

            var netPerOne = Math.Round(price.Gross / (1m + product.TaxRate!.Rate / 100m), 2);
            var vatPerOne = price.Gross - netPerOne;

            string? selectedModifiers = null;
            if (line.ModifierIds is { Count: > 0 })
            {
                var names = await db.Modifiers
                    .Where(m => line.ModifierIds.Contains(m.Id))
                    .Select(m => m.Name)
                    .ToListAsync();
                if (names.Count > 0)
                    selectedModifiers = string.Join(", ", names);
            }

            lines.Add(OrderLine.Create(orderId, line.ProductId, line.Quantity, netPerOne, vatPerOne, selectedModifiers));
            subtotal += price.Gross * line.Quantity;
        }

        return (lines, subtotal);
    }

    private static async Task ApplyPromotionAsync(string code, decimal subtotal, Order order, DateTimeOffset now, AppDbContext db)
    {
        var promo = await db.PromotionCodes.FirstOrDefaultAsync(x => x.Code == code)
            ?? throw new DomainException($"Promotion code '{code}' is not valid.");

        if (promo.ValidFrom is DateTimeOffset from && now < from)
            throw new DomainException("Promotion code is not yet active.");
        if (promo.ValidUntil is DateTimeOffset until && now > until)
            throw new DomainException("Promotion code has expired.");
        if (promo.MaxUses is int max && promo.UsesCount >= max)
            throw new DomainException("Promotion code has reached its usage cap.");

        var discount = Math.Round(subtotal * (promo.DiscountPercentage / 100m), 2);
        order.AssignPromotion(promo.Id, discount);
        promo.RegisterUsage();
    }

    private static async Task RedeemLoyaltyPointsAsync(Guid userId, int points, Guid orderId, AppDbContext db)
    {
        var balance = await db.LoyaltyPointLogs
            .Where(x => x.UserId == userId)
            .SumAsync(x => (int?)x.Points) ?? 0;
        if (balance < points)
            throw new DomainException($"Insufficient loyalty balance ({balance} available).");

        db.LoyaltyPointLogs.Add(LoyaltyPointLog.Create(userId, -points, reason: $"{LoyaltyReasons.RedeemedPrefix}{orderId}"));
    }
}
