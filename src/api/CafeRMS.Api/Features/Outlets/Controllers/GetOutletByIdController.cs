using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Outlets.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Outlets.Controllers;

[ApiController]
public sealed class GetOutletByIdController : ControllerBase
{
    [HttpGet("/api/outlets/{id:guid}")]
    [Authorize(Policy = Permissions.OutletManage)]
    public async Task<GetOutletById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetOutletById.Execute(id, db);
}
