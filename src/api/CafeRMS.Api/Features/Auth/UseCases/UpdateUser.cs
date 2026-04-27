using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Auth.UseCases;

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


public static class UpdateUser
{
    public sealed record Command(Guid CompanyId, Guid UserId, string FirstName, string LastName);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == command.UserId)
            ?? throw new NotFoundException("User not found.");

        if (user.AccountType != AccountType.Staff || user.CompanyId != command.CompanyId)
            throw new ForbiddenException("User is not a staff member of the current company.");

        user.UpdateProfile(command.FirstName, command.LastName);
        await db.SaveChangesAsync();
    }
}
