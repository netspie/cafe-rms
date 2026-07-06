using CafeRMS.Api.Features.Reports.UseCases;
using ClosedXML.Excel;

namespace CafeRMS.Api.Features.Reports;

public static class SalesPerProductExcel
{
    public static byte[] Render(GetSalesPerProduct.Result data)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Products");

        sheet.Cell(1, 1).Value = "Sales per Product";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 16;
        sheet.Range(1, 1, 1, 5).Merge();

        sheet.Cell(2, 1).Value = $"From {data.From:yyyy-MM-dd} to {data.To:yyyy-MM-dd}"
            + (data.EventId is null ? "" : " · single event");
        sheet.Range(2, 1, 2, 5).Merge();

        sheet.Cell(4, 1).Value = "Product";
        sheet.Cell(4, 2).Value = "Qty";
        sheet.Cell(4, 3).Value = "Net (PLN)";
        sheet.Cell(4, 4).Value = "VAT (PLN)";
        sheet.Cell(4, 5).Value = "Gross (PLN)";
        sheet.Range(4, 1, 4, 5).Style.Font.Bold = true;
        sheet.Range(4, 1, 4, 5).Style.Border.BottomBorder = XLBorderStyleValues.Thin;

        var row = 5;
        foreach (var product in data.Products)
        {
            sheet.Cell(row, 1).Value = product.ProductName;
            sheet.Cell(row, 2).Value = product.QuantitySold;
            sheet.Cell(row, 3).Value = product.Net;
            sheet.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            sheet.Cell(row, 4).Value = product.Vat;
            sheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            sheet.Cell(row, 5).Value = product.Gross;
            sheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            row++;
        }

        sheet.Cell(row, 1).Value = "Total";
        sheet.Cell(row, 2).Value = data.TotalItems;
        sheet.Cell(row, 3).Value = data.TotalNet;
        sheet.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
        sheet.Cell(row, 4).Value = data.TotalVat;
        sheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
        sheet.Cell(row, 5).Value = data.TotalGross;
        sheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
        sheet.Range(row, 1, row, 5).Style.Font.Bold = true;
        sheet.Range(row, 1, row, 5).Style.Border.TopBorder = XLBorderStyleValues.Thin;

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
