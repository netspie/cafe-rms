using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Tags.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Tags.Controllers;

[ApiController]
public sealed class GetTagByIdController : ControllerBase
{
    [HttpGet("/api/tags/{id:guid}")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<GetTagById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetTagById.Execute(id, db);
}
