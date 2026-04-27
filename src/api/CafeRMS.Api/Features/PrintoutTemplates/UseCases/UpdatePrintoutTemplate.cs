using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.PrintoutTemplates.UseCases;

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


public static class UpdatePrintoutTemplate
{
    public sealed record Command(Guid Id, string Name, string TemplateFileUrl);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var template = await db.PrintoutTemplates.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Printout template not found.");

        var nameTaken = await db.PrintoutTemplates.AnyAsync(x => x.Name == command.Name && x.Id != command.Id);
        if (nameTaken)
            throw new ConflictException($"A printout template named '{command.Name}' already exists.");

        template.Update(command.Name, command.TemplateFileUrl);
        await db.SaveChangesAsync();
    }
}
