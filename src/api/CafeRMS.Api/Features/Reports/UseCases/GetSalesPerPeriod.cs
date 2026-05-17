using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Reports.UseCases;

[ApiController]
public sealed class GetSalesPerPeriodController : ControllerBase
{
    [HttpGet("/api/reports/sales")]
    [Authorize(Policy = Permissions.ReportsView)]
    public Task<GetSalesPerPeriod.Result> Handle(
        [FromQuery] GetSalesPerPeriodRequest request,
        [FromServices] AppDbContext db) =>
        GetSalesPerPeriod.Execute(
            new GetSalesPerPeriod.Query(request.From, request.To, request.Granularity),
            db);
}

public sealed record GetSalesPerPeriodRequest(DateOnly From, DateOnly To, string Granularity = "day");

public sealed class GetSalesPerPeriodValidator : AbstractValidator<GetSalesPerPeriodRequest>
{
    public GetSalesPerPeriodValidator()
    {
        RuleFor(x => x.From).NotEmpty();
        RuleFor(x => x.To).NotEmpty()
            .GreaterThanOrEqualTo(x => x.From).WithMessage("'to' must be on or after 'from'.");
        RuleFor(x => x.Granularity)
            .Must(g => g is "day" or "month" or "quarter")
            .WithMessage("Granularity must be one of: day, month, quarter.");
    }
}

public static class GetSalesPerPeriod
{
    public sealed record Query(DateOnly From, DateOnly To, string Granularity);

    public sealed record Bucket(DateOnly Period, decimal Revenue, int OrdersCount, int ItemsSold);

    public sealed record Result(
        DateOnly From,
        DateOnly To,
        string Granularity,
        IReadOnlyList<Bucket> Buckets,
        decimal TotalRevenue,
        int TotalOrders,
        int TotalItems);

    public static async Task<Result> Execute(Query query, AppDbContext db)
    {
        var fromUtc = new DateTimeOffset(query.From.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var toExclusiveUtc = new DateTimeOffset(query.To.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        var lines = await db.Orders
            .Where(o => o.ClosedAt != null
                && o.CancelledAt == null
                && o.ClosedAt >= fromUtc
                && o.ClosedAt < toExclusiveUtc)
            .Join(db.OrderLines, o => o.Id, ol => ol.OrderId, (o, ol) => new
            {
                OrderId = o.Id,
                ClosedAt = o.ClosedAt!.Value,
                ol.Quantity,
                ol.NetPerOne,
                ol.VatPerOne
            })
            .ToListAsync();

        var buckets = lines
            .GroupBy(x => BucketStart(x.ClosedAt, query.Granularity))
            .OrderBy(g => g.Key)
            .Select(g => new Bucket(
                DateOnly.FromDateTime(g.Key),
                g.Sum(x => (x.NetPerOne + x.VatPerOne) * x.Quantity),
                g.Select(x => x.OrderId).Distinct().Count(),
                g.Sum(x => x.Quantity)))
            .ToList();

        return new Result(
            query.From,
            query.To,
            query.Granularity,
            buckets,
            buckets.Sum(x => x.Revenue),
            buckets.Sum(x => x.OrdersCount),
            buckets.Sum(x => x.ItemsSold));
    }

    private static DateTime BucketStart(DateTimeOffset closedAt, string granularity)
    {
        var date = closedAt.UtcDateTime.Date;
        return granularity switch
        {
            "month" => new DateTime(date.Year, date.Month, 1),
            "quarter" => new DateTime(date.Year, ((date.Month - 1) / 3) * 3 + 1, 1),
            _ => date
        };
    }
}
