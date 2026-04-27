using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Tags.UseCases;

[ApiController]
public sealed class DeleteTagController : ControllerBase
{
    [HttpDelete("/api/tags/{id:guid}")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await DeleteTag.Execute(id, db);
        return NoContent();
    }
}

public static class DeleteTag
{
    public static async Task Execute(Guid id, AppDbContext db)
    {
        var tag = await db.Tags.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Tag not found.");

        var links = await db.ProductTags.Where(x => x.TagId == id).ToListAsync();
        db.ProductTags.RemoveRange(links);

        db.Tags.Remove(tag);
        await db.SaveChangesAsync();
    }
}
