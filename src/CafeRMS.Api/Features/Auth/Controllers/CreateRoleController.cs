using CafeRMS.Api.Features.Auth.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Auth.Controllers;

[ApiController]
public sealed class CreateRoleController : ControllerBase
{
    [HttpPost("/api/roles")]
    [Authorize(Policy = Permissions.RolesManage)]
    public async Task<CreateRoleResponse> Handle(
        [FromBody] CreateRoleRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new CreateRole.Command(db.CurrentCompanyId, request.Name, request.Permissions ?? []);
        var result = await CreateRole.Execute(command, db);
        return new CreateRoleResponse(result.RoleId);
    }
}

public sealed record CreateRoleRequest(string Name, IReadOnlyList<string>? Permissions);

public sealed record CreateRoleResponse(Guid RoleId);

public sealed class CreateRoleValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
