using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Tables.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Tables.Controllers;

[ApiController]
public sealed class GetTableByIdController : ControllerBase
{
    [HttpGet("/api/tables/{id:guid}")]
    [Authorize(Policy = Permissions.TablesManage)]
    public async Task<GetTableById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetTableById.Execute(id, db);
}
