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
        var buckets = await db.Database
            .SqlQuery<BucketRow>(
                $"SELECT period, revenue, orders_count, items_sold FROM fn_sales_per_period({query.From}, {query.To}, {query.Granularity})")
            .ToListAsync();

        var mapped = buckets
            .Select(x => new Bucket(x.Period, x.Revenue, x.OrdersCount, x.ItemsSold))
            .ToList();

        return new Result(
            query.From,
            query.To,
            query.Granularity,
            mapped,
            mapped.Sum(x => x.Revenue),
            mapped.Sum(x => x.OrdersCount),
            mapped.Sum(x => x.ItemsSold));
    }

    private sealed record BucketRow(DateOnly Period, decimal Revenue, int OrdersCount, int ItemsSold);
}
