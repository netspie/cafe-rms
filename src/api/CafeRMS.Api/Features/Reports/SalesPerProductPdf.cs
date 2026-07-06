using CafeRMS.Api.Features.Reports.UseCases;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CafeRMS.Api.Features.Reports;

public static class SalesPerProductPdf
{
    public static byte[] Render(GetSalesPerProduct.Result data) =>
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
                        col.Item().Text("Sales per Product").FontSize(20).Bold();
                        var subtitle = $"From {data.From:yyyy-MM-dd} to {data.To:yyyy-MM-dd}"
                            + (data.EventId is null ? "" : " · single event");
                        col.Item().Text(subtitle).FontColor(Colors.Grey.Darken1);
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

    private static void Totals(IContainer container, GetSalesPerProduct.Result data) =>
        container.Row(row =>
        {
            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
            {
                c.Item().Text("Gross revenue").FontColor(Colors.Grey.Darken1).FontSize(9);
                c.Item().Text($"{data.TotalGross:N2} PLN").FontSize(16).Bold();
            });
            row.ConstantItem(10);
            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
            {
                c.Item().Text("Items sold").FontColor(Colors.Grey.Darken1).FontSize(9);
                c.Item().Text($"{data.TotalItems}").FontSize(16).Bold();
            });
            row.ConstantItem(10);
            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
            {
                c.Item().Text("Products").FontColor(Colors.Grey.Darken1).FontSize(9);
                c.Item().Text($"{data.Products.Count}").FontSize(16).Bold();
            });
        });

    private static void Table(IContainer container, GetSalesPerProduct.Result data) =>
        container.Table(t =>
        {
            t.ColumnsDefinition(c =>
            {
                c.RelativeColumn(3);
                c.RelativeColumn(1);
                c.RelativeColumn(1);
                c.RelativeColumn(1);
                c.RelativeColumn(1);
            });

            t.Header(h =>
            {
                h.Cell().Element(HeaderCell).Text("Product");
                h.Cell().Element(HeaderCell).AlignRight().Text("Qty");
                h.Cell().Element(HeaderCell).AlignRight().Text("Net (PLN)");
                h.Cell().Element(HeaderCell).AlignRight().Text("VAT (PLN)");
                h.Cell().Element(HeaderCell).AlignRight().Text("Gross (PLN)");
            });

            foreach (var product in data.Products)
            {
                t.Cell().Element(BodyCell).Text(product.ProductName);
                t.Cell().Element(BodyCell).AlignRight().Text($"{product.QuantitySold}");
                t.Cell().Element(BodyCell).AlignRight().Text($"{product.Net:N2}");
                t.Cell().Element(BodyCell).AlignRight().Text($"{product.Vat:N2}");
                t.Cell().Element(BodyCell).AlignRight().Text($"{product.Gross:N2}");
            }
        });

    private static IContainer HeaderCell(IContainer container) =>
        container.DefaultTextStyle(t => t.SemiBold())
            .PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Medium);

    private static IContainer BodyCell(IContainer container) =>
        container.PaddingVertical(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
}
