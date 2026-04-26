using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Modifiers.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Modifiers.Controllers;

[ApiController]
public sealed class GetModifierByIdController : ControllerBase
{
    [HttpGet("/api/modifiers/{id:guid}")]
    [Authorize(Policy = Permissions.ModifiersManage)]
    public async Task<GetModifierById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetModifierById.Execute(id, db);
}
