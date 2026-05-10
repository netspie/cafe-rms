using CafeRMS.Api.Features.Reports.UseCases;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CafeRMS.Api.Features.Reports;

public static class SalesReportPdf
{
    public static byte[] Render(GetSalesPerPeriod.Result data) =>
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
                        col.Item().Text("Sales Report").FontSize(20).Bold();
                        col.Item().Text($"From {data.From:yyyy-MM-dd} to {data.To:yyyy-MM-dd} · grouped by {data.Granularity}")
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

    private static void Totals(IContainer container, GetSalesPerPeriod.Result data) =>
        container.Row(row =>
        {
            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
            {
                c.Item().Text("Total revenue").FontColor(Colors.Grey.Darken1).FontSize(9);
                c.Item().Text($"{data.TotalRevenue:N2} PLN").FontSize(16).Bold();
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
                c.Item().Text("Items sold").FontColor(Colors.Grey.Darken1).FontSize(9);
                c.Item().Text($"{data.TotalItems}").FontSize(16).Bold();
            });
        });

    private static void Table(IContainer container, GetSalesPerPeriod.Result data) =>
        container.Table(t =>
        {
            t.ColumnsDefinition(c =>
            {
                c.RelativeColumn(2);
                c.RelativeColumn(2);
                c.RelativeColumn(1);
                c.RelativeColumn(1);
            });

            t.Header(h =>
            {
                h.Cell().Element(HeaderCell).Text("Period");
                h.Cell().Element(HeaderCell).AlignRight().Text("Revenue (PLN)");
                h.Cell().Element(HeaderCell).AlignRight().Text("Orders");
                h.Cell().Element(HeaderCell).AlignRight().Text("Items");
            });

            foreach (var bucket in data.Buckets)
            {
                t.Cell().Element(BodyCell).Text($"{bucket.Period:yyyy-MM-dd}");
                t.Cell().Element(BodyCell).AlignRight().Text($"{bucket.Revenue:N2}");
                t.Cell().Element(BodyCell).AlignRight().Text($"{bucket.OrdersCount}");
                t.Cell().Element(BodyCell).AlignRight().Text($"{bucket.ItemsSold}");
            }
        });

    private static IContainer HeaderCell(IContainer container) =>
        container.DefaultTextStyle(t => t.SemiBold())
            .PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Medium);

    private static IContainer BodyCell(IContainer container) =>
        container.PaddingVertical(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
}
