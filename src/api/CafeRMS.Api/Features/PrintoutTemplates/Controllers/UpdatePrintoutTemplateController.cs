using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.PrintoutTemplates.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.PrintoutTemplates.Controllers;

[ApiController]
public sealed class UpdatePrintoutTemplateController : ControllerBase
{
    [HttpPut("/api/printout-templates/{id:guid}")]
    [Authorize(Policy = Permissions.PrintoutTemplatesManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdatePrintoutTemplateRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new UpdatePrintoutTemplate.Command(id, request.Name, request.TemplateFileUrl);
        await UpdatePrintoutTemplate.Execute(command, db);
        return NoContent();
    }
}

public sealed record UpdatePrintoutTemplateRequest(string Name, string TemplateFileUrl);

public sealed class UpdatePrintoutTemplateValidator : AbstractValidator<UpdatePrintoutTemplateRequest>
{
    public UpdatePrintoutTemplateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TemplateFileUrl).NotEmpty().MaximumLength(500);
    }
}
