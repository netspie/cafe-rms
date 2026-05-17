using CafeRMS.Api.Features.Reports.UseCases;
using ClosedXML.Excel;

namespace CafeRMS.Api.Features.Reports;

public static class EventAttendanceReportExcel
{
    public static byte[] Render(GetEventAttendance.Result data)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Events");

        sheet.Cell(1, 1).Value = "Event Attendance Report";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 16;
        sheet.Range(1, 1, 1, 7).Merge();

        sheet.Cell(2, 1).Value = $"From {data.From:yyyy-MM-dd} to {data.To:yyyy-MM-dd}";
        sheet.Range(2, 1, 2, 7).Merge();

        sheet.Cell(4, 1).Value = "Event";
        sheet.Cell(4, 2).Value = "Dates";
        sheet.Cell(4, 3).Value = "Days";
        sheet.Cell(4, 4).Value = "Attendees";
        sheet.Cell(4, 5).Value = "Orders";
        sheet.Cell(4, 6).Value = "Items sold";
        sheet.Cell(4, 7).Value = "Revenue (PLN)";
        sheet.Range(4, 1, 4, 7).Style.Font.Bold = true;
        sheet.Range(4, 1, 4, 7).Style.Border.BottomBorder = XLBorderStyleValues.Thin;

        var row = 5;
        foreach (var entry in data.Events)
        {
            var dates = entry.FirstDay is null
                ? "—"
                : (entry.FirstDay == entry.LastDay
                    ? $"{entry.FirstDay:yyyy-MM-dd}"
                    : $"{entry.FirstDay:yyyy-MM-dd} → {entry.LastDay:yyyy-MM-dd}");

            sheet.Cell(row, 1).Value = entry.EventName;
            sheet.Cell(row, 2).Value = dates;
            sheet.Cell(row, 3).Value = entry.DayCount;
            sheet.Cell(row, 4).Value = entry.AttendeesCount;
            sheet.Cell(row, 5).Value = entry.OrdersCount;
            sheet.Cell(row, 6).Value = entry.ItemsSold;
            sheet.Cell(row, 7).Value = entry.Revenue;
            sheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
            row++;
        }

        sheet.Cell(row, 1).Value = "Total";
        sheet.Cell(row, 3).Value = data.Events.Sum(x => x.DayCount);
        sheet.Cell(row, 4).Value = data.TotalAttendees;
        sheet.Cell(row, 5).Value = data.TotalOrders;
        sheet.Cell(row, 6).Value = data.TotalItems;
        sheet.Cell(row, 7).Value = data.TotalRevenue;
        sheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
        sheet.Range(row, 1, row, 7).Style.Font.Bold = true;
        sheet.Range(row, 1, row, 7).Style.Border.TopBorder = XLBorderStyleValues.Thin;

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
