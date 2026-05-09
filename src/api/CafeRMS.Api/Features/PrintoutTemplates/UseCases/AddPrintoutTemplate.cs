using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.PrintoutTemplates.UseCases;

[ApiController]
public sealed class AddPrintoutTemplateController : ControllerBase
{
    [HttpPost("/api/printout-templates")]
    [Authorize(Policy = Permissions.PrintoutTemplatesManage)]
    public async Task<AddPrintoutTemplateResponse> Handle(
        [FromBody] AddPrintoutTemplateRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new AddPrintoutTemplate.Command(request.Name, request.TemplateFileUrl);
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


public static class AddPrintoutTemplate
{
    public sealed record Command(string Name, string TemplateFileUrl);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        var nameTaken = await db.PrintoutTemplates.AnyAsync(x => x.Name == command.Name);
        if (nameTaken)
            throw new ConflictException($"A printout template named '{command.Name}' already exists.");

        var template = PrintoutTemplate.Create(command.Name, command.TemplateFileUrl);
        db.PrintoutTemplates.Add(template);
        await db.SaveChangesAsync();
        return new Result(template.Id);
    }
}
