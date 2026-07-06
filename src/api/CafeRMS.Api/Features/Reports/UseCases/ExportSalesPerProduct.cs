using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Reports.UseCases;

[ApiController]
public sealed class ExportSalesPerProductPdfController : ControllerBase
{
    [HttpGet("/api/reports/products/export.pdf")]
    [Authorize(Policy = Permissions.ReportsView)]
    public async Task<IActionResult> Handle(
        [FromQuery] GetSalesPerProductRequest request,
        [FromServices] AppDbContext db)
    {
        var data = await GetSalesPerProduct.Execute(new GetSalesPerProduct.Query(request.From, request.To, request.EventId), db);
        var bytes = SalesPerProductPdf.Render(data);
        var filename = $"products_{request.From:yyyyMMdd}_{request.To:yyyyMMdd}.pdf";

        return File(bytes, "application/pdf", filename);
    }
}

[ApiController]
public sealed class ExportSalesPerProductExcelController : ControllerBase
{
    [HttpGet("/api/reports/products/export.xlsx")]
    [Authorize(Policy = Permissions.ReportsView)]
    public async Task<IActionResult> Handle(
        [FromQuery] GetSalesPerProductRequest request,
        [FromServices] AppDbContext db)
    {
        var data = await GetSalesPerProduct.Execute(new GetSalesPerProduct.Query(request.From, request.To, request.EventId), db);
        var bytes = SalesPerProductExcel.Render(data);
        var filename = $"products_{request.From:yyyyMMdd}_{request.To:yyyyMMdd}.xlsx";

        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
    }
}
