using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Reports.UseCases;

[ApiController]
public sealed class ExportSalesPerPeriodPdfController : ControllerBase
{
    [HttpGet("/api/reports/sales/export.pdf")]
    [Authorize(Policy = Permissions.ReportsView)]
    public async Task<IActionResult> Handle(
        [FromQuery] GetSalesPerPeriodRequest request,
        [FromServices] AppDbContext db)
    {
        var data = await GetSalesPerPeriod.Execute(
            new GetSalesPerPeriod.Query(request.From, request.To, request.Granularity), db);
        var bytes = SalesReportPdf.Render(data);
        var filename = $"sales_{request.From:yyyyMMdd}_{request.To:yyyyMMdd}_{request.Granularity}.pdf";
        return File(bytes, "application/pdf", filename);
    }
}

[ApiController]
public sealed class ExportSalesPerPeriodExcelController : ControllerBase
{
    [HttpGet("/api/reports/sales/export.xlsx")]
    [Authorize(Policy = Permissions.ReportsView)]
    public async Task<IActionResult> Handle(
        [FromQuery] GetSalesPerPeriodRequest request,
        [FromServices] AppDbContext db)
    {
        var data = await GetSalesPerPeriod.Execute(
            new GetSalesPerPeriod.Query(request.From, request.To, request.Granularity), db);
        var bytes = SalesReportExcel.Render(data);
        var filename = $"sales_{request.From:yyyyMMdd}_{request.To:yyyyMMdd}_{request.Granularity}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
    }
}
