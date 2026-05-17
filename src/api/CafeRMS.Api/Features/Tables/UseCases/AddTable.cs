using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Tables.UseCases;

[ApiController]
public sealed class AddTableController : ControllerBase
{
    [HttpPost("/api/tables")]
    [Authorize(Policy = Permissions.TablesManage)]
    public async Task<AddTableResponse> Handle(
        [FromBody] AddTableRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new AddTable.Command(request.Name);
        var result = await AddTable.Execute(command, db);
        return new AddTableResponse(result.Id);
    }
}

public sealed record AddTableRequest(string Name);

public sealed record AddTableResponse(Guid Id);

public sealed class AddTableValidator : AbstractValidator<AddTableRequest>
{
    public AddTableValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}


public static class AddTable
{
    public sealed record Command(string Name);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        var outletId = await db.Outlets
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Outlet not found.");

        var nameTaken = await db.Tables.AnyAsync(x => x.Name == command.Name);
        if (nameTaken)
            throw new ConflictException($"A table named '{command.Name}' already exists.");

        var table = Table.Create(command.Name, outletId);
        db.Tables.Add(table);
        await db.SaveChangesAsync();
        return new Result(table.Id);
    }
}
