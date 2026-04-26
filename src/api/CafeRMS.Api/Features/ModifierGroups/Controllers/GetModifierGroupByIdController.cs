using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.ModifierGroups.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.ModifierGroups.Controllers;

[ApiController]
public sealed class GetModifierGroupByIdController : ControllerBase
{
    [HttpGet("/api/modifier-groups/{id:guid}")]
    [Authorize(Policy = Permissions.ModifiersManage)]
    public async Task<GetModifierGroupById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetModifierGroupById.Execute(id, db);
}
