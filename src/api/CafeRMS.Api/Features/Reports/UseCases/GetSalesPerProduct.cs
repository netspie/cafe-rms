using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Reports.UseCases;

[ApiController]
public sealed class GetSalesPerProductController : ControllerBase
{
    [HttpGet("/api/reports/products")]
    [Authorize(Policy = Permissions.ReportsView)]
    public Task<GetSalesPerProduct.Result> Handle(
        [FromQuery] GetSalesPerProductRequest request,
        [FromServices] AppDbContext db) =>
        GetSalesPerProduct.Execute(
            new GetSalesPerProduct.Query(request.From, request.To, request.EventId),
            db);
}

public sealed record GetSalesPerProductRequest(DateOnly From, DateOnly To, Guid? EventId = null);

public sealed class GetSalesPerProductValidator : AbstractValidator<GetSalesPerProductRequest>
{
    public GetSalesPerProductValidator()
    {
        RuleFor(x => x.From).NotEmpty();
        RuleFor(x => x.To).NotEmpty()
            .GreaterThanOrEqualTo(x => x.From).WithMessage("'to' must be on or after 'from'.");
    }
}

public static class GetSalesPerProduct
{
    public sealed record Query(DateOnly From, DateOnly To, Guid? EventId);

    public sealed record Row(
        Guid ProductId,
        string ProductName,
        int QuantitySold,
        decimal Net,
        decimal Vat,
        decimal Gross);

    public sealed record Result(
        DateOnly From,
        DateOnly To,
        Guid? EventId,
        IReadOnlyList<Row> Products,
        int TotalItems,
        decimal TotalNet,
        decimal TotalVat,
        decimal TotalGross);

    public static async Task<Result> Execute(Query query, AppDbContext db)
    {
        var fromUtc = new DateTimeOffset(query.From.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var toExclusiveUtc = new DateTimeOffset(query.To.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        var orders = db.Orders
            .Where(o => o.ClosedAt != null
                && o.CancelledAt == null
                && o.ClosedAt >= fromUtc
                && o.ClosedAt < toExclusiveUtc);

        if (query.EventId is Guid eventId)
            orders = orders.Where(o => o.EventId == eventId);

        var lines = await orders
            .Join(db.OrderLines, o => o.Id, ol => ol.OrderId, (o, ol) => ol)
            .Join(db.Products, ol => ol.ProductId, p => p.Id, (ol, p) => new
            {
                p.Id,
                p.Name,
                ol.Quantity,
                ol.NetPerOne,
                ol.VatPerOne
            })
            .ToListAsync();

        var products = lines
            .GroupBy(x => new { x.Id, x.Name })
            .Select(g => new Row(
                g.Key.Id,
                g.Key.Name,
                g.Sum(x => x.Quantity),
                g.Sum(x => x.NetPerOne * x.Quantity),
                g.Sum(x => x.VatPerOne * x.Quantity),
                g.Sum(x => (x.NetPerOne + x.VatPerOne) * x.Quantity)))
            .OrderByDescending(r => r.Gross)
            .ToList();

        return new Result(
            query.From,
            query.To,
            query.EventId,
            products,
            products.Sum(x => x.QuantitySold),
            products.Sum(x => x.Net),
            products.Sum(x => x.Vat),
            products.Sum(x => x.Gross));
    }
}
