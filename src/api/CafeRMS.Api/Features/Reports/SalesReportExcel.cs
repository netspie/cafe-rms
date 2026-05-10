using CafeRMS.Api.Features.Reports.UseCases;
using ClosedXML.Excel;

namespace CafeRMS.Api.Features.Reports;

public static class SalesReportExcel
{
    public static byte[] Render(GetSalesPerPeriod.Result data)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Sales");

        sheet.Cell(1, 1).Value = "Sales Report";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 16;
        sheet.Range(1, 1, 1, 4).Merge();

        sheet.Cell(2, 1).Value = $"From {data.From:yyyy-MM-dd} to {data.To:yyyy-MM-dd} · {data.Granularity}";
        sheet.Range(2, 1, 2, 4).Merge();

        sheet.Cell(4, 1).Value = "Period";
        sheet.Cell(4, 2).Value = "Revenue (PLN)";
        sheet.Cell(4, 3).Value = "Orders";
        sheet.Cell(4, 4).Value = "Items sold";
        sheet.Range(4, 1, 4, 4).Style.Font.Bold = true;
        sheet.Range(4, 1, 4, 4).Style.Border.BottomBorder = XLBorderStyleValues.Thin;

        var row = 5;
        foreach (var bucket in data.Buckets)
        {
            sheet.Cell(row, 1).Value = bucket.Period.ToDateTime(TimeOnly.MinValue);
            sheet.Cell(row, 1).Style.DateFormat.Format = "yyyy-MM-dd";
            sheet.Cell(row, 2).Value = bucket.Revenue;
            sheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00";
            sheet.Cell(row, 3).Value = bucket.OrdersCount;
            sheet.Cell(row, 4).Value = bucket.ItemsSold;
            row++;
        }

        sheet.Cell(row, 1).Value = "Total";
        sheet.Cell(row, 2).Value = data.TotalRevenue;
        sheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00";
        sheet.Cell(row, 3).Value = data.TotalOrders;
        sheet.Cell(row, 4).Value = data.TotalItems;
        sheet.Range(row, 1, row, 4).Style.Font.Bold = true;
        sheet.Range(row, 1, row, 4).Style.Border.TopBorder = XLBorderStyleValues.Thin;

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
