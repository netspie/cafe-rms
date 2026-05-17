using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Reports.UseCases;

[ApiController]
public sealed class ExportEventAttendancePdfController : ControllerBase
{
    [HttpGet("/api/reports/events/export.pdf")]
    [Authorize(Policy = Permissions.ReportsView)]
    public async Task<IActionResult> Handle(
        [FromQuery] GetEventAttendanceRequest request,
        [FromServices] AppDbContext db)
    {
        var data = await GetEventAttendance.Execute(new GetEventAttendance.Query(request.From, request.To), db);
        var bytes = EventAttendanceReportPdf.Render(data);
        var filename = $"events_{request.From:yyyyMMdd}_{request.To:yyyyMMdd}.pdf";

        return File(bytes, "application/pdf", filename);
    }
}

[ApiController]
public sealed class ExportEventAttendanceExcelController : ControllerBase
{
    [HttpGet("/api/reports/events/export.xlsx")]
    [Authorize(Policy = Permissions.ReportsView)]
    public async Task<IActionResult> Handle(
        [FromQuery] GetEventAttendanceRequest request,
        [FromServices] AppDbContext db)
    {
        var data = await GetEventAttendance.Execute(new GetEventAttendance.Query(request.From, request.To), db);
        var bytes = EventAttendanceReportExcel.Render(data);
        var filename = $"events_{request.From:yyyyMMdd}_{request.To:yyyyMMdd}.xlsx";

        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
    }
}
