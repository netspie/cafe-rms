using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Tables.UseCases;

[ApiController]
public sealed class DeleteTableController : ControllerBase
{
    [HttpDelete("/api/tables/{id:guid}")]
    [Authorize(Policy = Permissions.TablesManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await DeleteTable.Execute(id, db);
        return NoContent();
    }
}


public static class DeleteTable
{
    public static async Task Execute(Guid id, AppDbContext db)
    {
        var table = await db.Tables.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Table not found.");

        db.Tables.Remove(table);
        await db.SaveChangesAsync();
    }
}
