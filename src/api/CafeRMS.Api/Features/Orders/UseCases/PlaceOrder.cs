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

// Backs both POST /api/my/orders (Guest, UserId from JWT) and POST /api/orders
// (Staff walk-in, optional UserId). Wraps everything in a transaction so a failure
// at any step rolls back the order, the lines, the loyalty debit, and the promo
// usage increment together.
//
// Why IgnoreQueryFilters everywhere: Guest callers have no company in their JWT, so
// db.CurrentCompanyId resolves to Guid.Empty and the ICompanyOwned global filter hides
// every cross-referenced entity (Outlet, Product, PriceGroup, PromotionCode). We bypass
// the filters and derive CompanyId from the target outlet, then manually filter every
// subsequent lookup against that CompanyId. CallerCompanyId (set for Staff, null for
// Guest) makes Staff cross-tenant id leaks → 404.
public static class PlaceOrder
{
    public sealed record LineInput(Guid ProductId, int Quantity, Guid? PriceGroupId);

    public sealed record Command(
        Guid OutletId,
        Guid? CallerCompanyId,
        Guid? UserId,
        Guid? TableId,
        Guid? SalesChannelId,
        Guid? EventId,
        string? PromotionCode,
        int LoyaltyPointsUsed,
        IReadOnlyList<LineInput> Lines);

    public sealed record Result(Guid OrderId);

    public static async Task<Result> Execute(Command command, AppDbContext db, DateTimeOffset now)
    {
        if (command.Lines.Count == 0)
            throw new DomainException("Order must have at least one line.");

        if (command.LoyaltyPointsUsed < 0)
            throw new DomainException("LoyaltyPointsUsed must be zero or positive.");

        var outlet = await db.Outlets.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == command.OutletId && x.DeletedAt == null)
            ?? throw new NotFoundException("Outlet not found.");

        var companyId = outlet.CompanyId;

        // Staff cross-tenant attempt → 404 (don't leak that another tenant's outlet exists).
        if (command.CallerCompanyId is { } caller && caller != companyId)
            throw new NotFoundException("Outlet not found.");

        await using var tx = await db.Database.BeginTransactionAsync();

        var order = Order.Create(
            command.OutletId,
            command.TableId,
            command.SalesChannelId,
            command.UserId,
            command.EventId,
            promotionCodeId: null,
            discount: 0m,
            loyaltyPointsUsed: command.LoyaltyPointsUsed);

        var orderLines = new List<OrderLine>();
        decimal subtotal = 0m;

        // Resolve every line: pick a ProductPrice (caller-specified PriceGroup if given,
        // else FIFO any price for the product) and snapshot Net + VAT into the OrderLine.
        foreach (var line in command.Lines)
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
        if (!string.IsNullOrWhiteSpace(command.PromotionCode))
        {
            var promo = await db.PromotionCodes.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Code == command.PromotionCode && x.CompanyId == companyId && x.DeletedAt == null)
                ?? throw new DomainException($"Promotion code '{command.PromotionCode}' is not valid.");

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
        // inserting a negative LoyaltyPointLog. Walk-ins (no UserId) can't redeem.
        if (command.LoyaltyPointsUsed > 0)
        {
            if (command.UserId is not Guid userIdForLoyalty)
                throw new DomainException("Walk-in orders cannot redeem loyalty points.");

            var balance = await db.LoyaltyPointLogs.IgnoreQueryFilters()
                .Where(x => x.UserId == userIdForLoyalty)
                .SumAsync(x => (int?)x.Points) ?? 0;
            if (balance < command.LoyaltyPointsUsed)
                throw new DomainException($"Insufficient loyalty balance ({balance} available).");

            db.LoyaltyPointLogs.Add(LoyaltyPointLog.Create(
                userIdForLoyalty,
                -command.LoyaltyPointsUsed,
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
