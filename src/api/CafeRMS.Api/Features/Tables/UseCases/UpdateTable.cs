using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Tables.UseCases;

[ApiController]
public sealed class UpdateTableController : ControllerBase
{
    [HttpPut("/api/tables/{id:guid}")]
    [Authorize(Policy = Permissions.TablesManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdateTableRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new UpdateTable.Command(id, request.Name);
        await UpdateTable.Execute(command, db);
        return NoContent();
    }
}

public sealed record UpdateTableRequest(string Name);

public sealed class UpdateTableValidator : AbstractValidator<UpdateTableRequest>
{
    public UpdateTableValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}


public static class UpdateTable
{
    public sealed record Command(Guid Id, string Name);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var table = await db.Tables.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Table not found.");

        var nameTaken = await db.Tables.AnyAsync(x => x.Name == command.Name && x.Id != command.Id);
        if (nameTaken)
            throw new ConflictException($"A table named '{command.Name}' already exists.");

        table.Update(command.Name);
        await db.SaveChangesAsync();
    }
}
