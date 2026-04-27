using CafeRMS.Api.Features.Auth;
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

// Single endpoint: customer places an order from the mobile app. Wraps everything in a
// transaction so a failure at any step rolls back the order, the lines, the loyalty
// debit, and the promo usage increment together.
//
// Why IgnoreQueryFilters everywhere: Guest callers have no company in their JWT, so
// db.CurrentCompanyId resolves to Guid.Empty and the ICompanyOwned global filter hides
// every cross-referenced entity (Outlet, Product, PriceGroup, PromotionCode). We bypass
// the filters and derive CompanyId from the target outlet, then manually filter every
// subsequent lookup against that CompanyId.
public static class PlaceOrder
{
    public sealed record Result(Guid OrderId);

    public static async Task<Result> Execute(PlaceOrderRequest request, Guid userId, AppDbContext db, DateTimeOffset now)
    {
        if (request.Lines.Count == 0)
            throw new DomainException("Order must have at least one line.");

        var outlet = await db.Outlets.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.OutletId && x.DeletedAt == null)
            ?? throw new NotFoundException("Outlet not found.");

        var companyId = outlet.CompanyId;

        await using var tx = await db.Database.BeginTransactionAsync();

        var order = Order.Create(
            request.OutletId,
            request.TableId,
            request.SalesChannelId,
            userId,
            request.EventId,
            promotionCodeId: null,
            discount: 0m,
            loyaltyPointsUsed: request.LoyaltyPointsUsed);

        var orderLines = new List<OrderLine>();
        decimal subtotal = 0m;

        // Resolve every line: pick a ProductPrice (caller-specified PriceGroup if given,
        // else FIFO any price for the product) and snapshot Net + VAT into the OrderLine.
        foreach (var line in request.Lines)
        {
            if (line.Quantity <= 0)
                throw new DomainException("Quantity must be positive.");

            var product = await db.Products.IgnoreQueryFilters()
                .Include(x => x.TaxRate)
                .FirstOrDefaultAsync(x => x.Id == line.ProductId && x.CompanyId == companyId && x.DeletedAt == null)
                ?? throw new NotFoundException($"Product {line.ProductId} not found.");

            var priceQuery = db.ProductPrices.IgnoreQueryFilters().Where(x => x.ProductId == line.ProductId);
            if (line.PriceGroupId is { } pgId)
                priceQuery = priceQuery.Where(x => x.PriceGroupId == pgId);
            var price = await priceQuery.FirstOrDefaultAsync()
                ?? throw new NotFoundException($"No price set for product {line.ProductId}.");

            var vatPerOne = Math.Round(price.Net * (product.TaxRate!.Rate / 100m), 2);
            orderLines.Add(OrderLine.Create(order.Id, line.ProductId, line.Quantity, price.Net, vatPerOne));
            subtotal += (price.Net + vatPerOne) * line.Quantity;
        }

        // Promotion code: validate, snapshot the FK on the order, increment UsesCount.
        if (!string.IsNullOrWhiteSpace(request.PromotionCode))
        {
            var promo = await db.PromotionCodes.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Code == request.PromotionCode && x.CompanyId == companyId && x.DeletedAt == null)
                ?? throw new DomainException($"Promotion code '{request.PromotionCode}' is not valid.");

            if (promo.ValidFrom is { } from && now < from)
                throw new DomainException("Promotion code is not yet active.");
            if (promo.ValidUntil is { } until && now > until)
                throw new DomainException("Promotion code has expired.");
            if (promo.MaxUses is { } max && promo.UsesCount >= max)
                throw new DomainException("Promotion code has reached its usage cap.");

            var discount = Math.Round(subtotal * (promo.DiscountPercentage / 100m), 2);
            order.AssignPromotion(promo.Id, discount);
            promo.RegisterUsage();
        }

        // Loyalty redemption: validate balance via SUM(log) and burn the points by
        // inserting a negative LoyaltyPointLog.
        if (request.LoyaltyPointsUsed > 0)
        {
            var balance = await db.LoyaltyPointLogs.IgnoreQueryFilters()
                .Where(x => x.UserId == userId)
                .SumAsync(x => (int?)x.Points) ?? 0;
            if (balance < request.LoyaltyPointsUsed)
                throw new DomainException($"Insufficient loyalty balance ({balance} available).");

            db.LoyaltyPointLogs.Add(LoyaltyPointLog.Create(
                userId,
                -request.LoyaltyPointsUsed,
                companyId,
                reason: $"Redeemed on order {order.Id}"));
        }

        db.Orders.Add(order);
        db.OrderLines.AddRange(orderLines);

        await db.SaveChangesAsync();
        await tx.CommitAsync();

        return new Result(order.Id);
    }
}
