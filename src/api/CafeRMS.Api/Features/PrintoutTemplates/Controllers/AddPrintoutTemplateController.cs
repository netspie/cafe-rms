using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.PrintoutTemplates.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.PrintoutTemplates.Controllers;

[ApiController]
public sealed class AddPrintoutTemplateController : ControllerBase
{
    [HttpPost("/api/printout-templates")]
    [Authorize(Policy = Permissions.PrintoutTemplatesManage)]
    public async Task<AddPrintoutTemplateResponse> Handle(
        [FromBody] AddPrintoutTemplateRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new AddPrintoutTemplate.Command(db.CurrentCompanyId, request.Name, request.TemplateFileUrl);
        var result = await AddPrintoutTemplate.Execute(command, db);
        return new AddPrintoutTemplateResponse(result.Id);
    }
}

public sealed record AddPrintoutTemplateRequest(string Name, string TemplateFileUrl);

public sealed record AddPrintoutTemplateResponse(Guid Id);

public sealed class AddPrintoutTemplateValidator : AbstractValidator<AddPrintoutTemplateRequest>
{
    public AddPrintoutTemplateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TemplateFileUrl).NotEmpty().MaximumLength(500);
    }
}
