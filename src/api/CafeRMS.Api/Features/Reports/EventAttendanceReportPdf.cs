using CafeRMS.Api.Features.Reports.UseCases;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CafeRMS.Api.Features.Reports;

public static class EventAttendanceReportPdf
{
    public static byte[] Render(GetEventAttendance.Result data) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(t => t.FontFamily("Helvetica").FontSize(10));

                page.Header().Element(header =>
                {
                    header.Column(col =>
                    {
                        col.Item().Text("Event Attendance Report").FontSize(20).Bold();
                        col.Item().Text($"From {data.From:yyyy-MM-dd} to {data.To:yyyy-MM-dd}")
                            .FontColor(Colors.Grey.Darken1);
                    });
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Spacing(15);

                    col.Item().Element(t => Totals(t, data));
                    col.Item().Element(t => Table(t, data));
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Page ");
                    t.CurrentPageNumber();
                    t.Span(" of ");
                    t.TotalPages();
                });
            });
        }).GeneratePdf();

    private static void Totals(IContainer container, GetEventAttendance.Result data) =>
        container.Row(row =>
        {
            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
            {
                c.Item().Text("Events").FontColor(Colors.Grey.Darken1).FontSize(9);
                c.Item().Text($"{data.TotalEvents}").FontSize(16).Bold();
            });
            row.ConstantItem(10);
            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
            {
                c.Item().Text("Attendees").FontColor(Colors.Grey.Darken1).FontSize(9);
                c.Item().Text($"{data.TotalAttendees}").FontSize(16).Bold();
            });
            row.ConstantItem(10);
            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
            {
                c.Item().Text("Orders").FontColor(Colors.Grey.Darken1).FontSize(9);
                c.Item().Text($"{data.TotalOrders}").FontSize(16).Bold();
            });
            row.ConstantItem(10);
            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
            {
                c.Item().Text("Revenue").FontColor(Colors.Grey.Darken1).FontSize(9);
                c.Item().Text($"{data.TotalRevenue:N2} PLN").FontSize(16).Bold();
            });
        });

    private static void Table(IContainer container, GetEventAttendance.Result data) =>
        container.Table(t =>
        {
            t.ColumnsDefinition(c =>
            {
                c.RelativeColumn(3);
                c.RelativeColumn(2);
                c.RelativeColumn(1);
                c.RelativeColumn(1);
                c.RelativeColumn(1);
                c.RelativeColumn(1);
                c.RelativeColumn(2);
            });

            t.Header(h =>
            {
                h.Cell().Element(HeaderCell).Text("Event");
                h.Cell().Element(HeaderCell).Text("Dates");
                h.Cell().Element(HeaderCell).AlignRight().Text("Days");
                h.Cell().Element(HeaderCell).AlignRight().Text("Attendees");
                h.Cell().Element(HeaderCell).AlignRight().Text("Orders");
                h.Cell().Element(HeaderCell).AlignRight().Text("Items");
                h.Cell().Element(HeaderCell).AlignRight().Text("Revenue (PLN)");
            });

            foreach (var row in data.Events)
            {
                var dates = row.FirstDay is null
                    ? "—"
                    : (row.FirstDay == row.LastDay
                        ? $"{row.FirstDay:yyyy-MM-dd}"
                        : $"{row.FirstDay:yyyy-MM-dd} → {row.LastDay:yyyy-MM-dd}");
                t.Cell().Element(BodyCell).Text(row.EventName);
                t.Cell().Element(BodyCell).Text(dates);
                t.Cell().Element(BodyCell).AlignRight().Text($"{row.DayCount}");
                t.Cell().Element(BodyCell).AlignRight().Text($"{row.AttendeesCount}");
                t.Cell().Element(BodyCell).AlignRight().Text($"{row.OrdersCount}");
                t.Cell().Element(BodyCell).AlignRight().Text($"{row.ItemsSold}");
                t.Cell().Element(BodyCell).AlignRight().Text($"{row.Revenue:N2}");
            }
        });

    private static IContainer HeaderCell(IContainer container) =>
        container.DefaultTextStyle(t => t.SemiBold())
            .PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Medium);

    private static IContainer BodyCell(IContainer container) =>
        container.PaddingVertical(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
}
