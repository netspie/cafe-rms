using CafeRMS.Api.Features.Auth.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Auth.Controllers;

[ApiController]
public sealed class UpdateUserController : ControllerBase
{
    [HttpPut("/api/users/{id:guid}")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdateUserRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new UpdateUser.Command(db.CurrentCompanyId, id, request.FirstName, request.LastName);
        await UpdateUser.Execute(command, db);
        return NoContent();
    }
}

public sealed record UpdateUserRequest(string FirstName, string LastName);

public sealed class UpdateUserValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
    }
}
