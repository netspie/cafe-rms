using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.PrintoutTemplates.UseCases;

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


public static class GetPrintoutTemplateById
{
    public sealed record Result(Guid Id, string Name, string TemplateFileUrl, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

    public static async Task<Result> Execute(Guid id, AppDbContext db)
    {
        var template = await db.PrintoutTemplates
            .Where(x => x.Id == id)
            .Select(x => new Result(x.Id, x.Name, x.TemplateFileUrl, x.CreatedAt, x.UpdatedAt))
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Printout template not found.");

        return template;
    }
}
