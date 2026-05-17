using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Reports.UseCases;

[ApiController]
public sealed class GetEventAttendanceController : ControllerBase
{
    [HttpGet("/api/reports/events")]
    [Authorize(Policy = Permissions.ReportsView)]
    public Task<GetEventAttendance.Result> Handle(
        [FromQuery] GetEventAttendanceRequest request,
        [FromServices] AppDbContext db) =>
        GetEventAttendance.Execute(new GetEventAttendance.Query(request.From, request.To), db);
}

public sealed record GetEventAttendanceRequest(DateOnly From, DateOnly To);

public sealed class GetEventAttendanceValidator : AbstractValidator<GetEventAttendanceRequest>
{
    public GetEventAttendanceValidator()
    {
        RuleFor(x => x.From).NotEmpty();
        RuleFor(x => x.To).NotEmpty()
            .GreaterThanOrEqualTo(x => x.From).WithMessage("'to' must be on or after 'from'.");
    }
}

public static class GetEventAttendance
{
    public sealed record Query(DateOnly From, DateOnly To);

    public sealed record Row(
        Guid EventId,
        string EventName,
        DateOnly? FirstDay,
        DateOnly? LastDay,
        int DayCount,
        int AttendeesCount,
        int OrdersCount,
        int ItemsSold,
        decimal Revenue);

    public sealed record Result(
        DateOnly From,
        DateOnly To,
        IReadOnlyList<Row> Events,
        int TotalEvents,
        int TotalAttendees,
        int TotalOrders,
        int TotalItems,
        decimal TotalRevenue);

    public static async Task<Result> Execute(Query query, AppDbContext db)
    {
        var eventDays = await db.EventDays
            .Where(d => d.Date >= query.From && d.Date <= query.To)
            .Select(d => new { d.EventId, d.Date })
            .ToListAsync();

        var eventIds = eventDays.Select(d => d.EventId).Distinct().ToList();
        if (eventIds.Count == 0)
            return new Result(query.From, query.To, [], 0, 0, 0, 0, 0m);

        var events = await db.Events
            .Where(e => eventIds.Contains(e.Id))
            .Select(e => new { e.Id, e.Name })
            .ToListAsync();

        var orderRows = await db.Orders
            .Where(o => o.EventId != null
                && eventIds.Contains(o.EventId!.Value)
                && o.ClosedAt != null
                && o.CancelledAt == null)
            .Join(db.OrderLines, o => o.Id, ol => ol.OrderId, (o, ol) => new
            {
                EventId = o.EventId!.Value,
                OrderId = o.Id,
                o.UserId,
                ol.Quantity,
                ol.NetPerOne,
                ol.VatPerOne
            })
            .ToListAsync();

        var rows = events
            .Select(e =>
            {
                var days = eventDays.Where(d => d.EventId == e.Id).Select(d => d.Date).OrderBy(d => d).ToList();
                var lines = orderRows.Where(o => o.EventId == e.Id).ToList();
                return new Row(
                    EventId: e.Id,
                    EventName: e.Name,
                    FirstDay: days.Count == 0 ? null : days.First(),
                    LastDay: days.Count == 0 ? null : days.Last(),
                    DayCount: days.Count,
                    AttendeesCount: lines.Where(x => x.UserId != null).Select(x => x.UserId).Distinct().Count(),
                    OrdersCount: lines.Select(x => x.OrderId).Distinct().Count(),
                    ItemsSold: lines.Sum(x => x.Quantity),
                    Revenue: lines.Sum(x => (x.NetPerOne + x.VatPerOne) * x.Quantity));
            })
            .OrderByDescending(r => r.FirstDay ?? DateOnly.MinValue)
            .ToList();

        return new Result(
            query.From,
            query.To,
            rows,
            rows.Count,
            rows.Sum(x => x.AttendeesCount),
            rows.Sum(x => x.OrdersCount),
            rows.Sum(x => x.ItemsSold),
            rows.Sum(x => x.Revenue));
    }
}
