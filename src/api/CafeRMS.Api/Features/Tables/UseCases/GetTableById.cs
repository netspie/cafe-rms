using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Tables.UseCases;

[ApiController]
public sealed class GetTableByIdController : ControllerBase
{
    [HttpGet("/api/tables/{id:guid}")]
    [Authorize]
    public async Task<GetTableById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetTableById.Execute(id, db);
}


public static class GetTableById
{
    public sealed record Result(Guid Id, string Name, Guid OutletId, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

    public static async Task<Result> Execute(Guid id, AppDbContext db)
    {
        var table = await db.Tables
            .Where(x => x.Id == id)
            .Select(x => new Result(x.Id, x.Name, x.OutletId, x.CreatedAt, x.UpdatedAt))
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Table not found.");

        return table;
    }
}
