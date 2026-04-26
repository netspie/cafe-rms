using CafeRMS.Api.Features.Auth.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Auth.Controllers;

[ApiController]
public sealed class UpdateRoleController : ControllerBase
{
    [HttpPut("/api/roles/{id:guid}")]
    [Authorize(Policy = Permissions.RolesManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdateRoleRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new UpdateRole.Command(id, request.Name, request.Permissions ?? []);
        await UpdateRole.Execute(command, db);
        return NoContent();
    }
}

public sealed record UpdateRoleRequest(string Name, IReadOnlyList<string>? Permissions);

public sealed class UpdateRoleValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
