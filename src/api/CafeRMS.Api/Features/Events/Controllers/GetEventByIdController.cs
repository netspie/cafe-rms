using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Events.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Events.Controllers;

[ApiController]
public sealed class GetEventByIdController : ControllerBase
{
    [HttpGet("/api/events/{id:guid}")]
    [Authorize(Policy = Permissions.EventsManage)]
    public async Task<GetEventById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetEventById.Execute(id, db);
}
