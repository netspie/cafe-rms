using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.PrintoutTemplates.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.PrintoutTemplates.Controllers;

[ApiController]
public sealed class GetPrintoutTemplateByIdController : ControllerBase
{
    [HttpGet("/api/printout-templates/{id:guid}")]
    [Authorize(Policy = Permissions.PrintoutTemplatesManage)]
    public async Task<GetPrintoutTemplateById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetPrintoutTemplateById.Execute(id, db);
}
