using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.UserSettings.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.UserSettings.Controllers;

[ApiController]
public sealed class UpdateMyUserSettingsController : ControllerBase
{
    [HttpPut("/api/my/settings")]
    [Authorize(Policy = Policies.RequireGuest)]
    public async Task<IActionResult> Handle(
        [FromBody] UpdateMyUserSettingsRequest request,
        [FromServices] AppDbContext db)
    {
        await UpdateMyUserSettings.Execute(new UpdateMyUserSettings.Command(User.UserId, request.Theme, request.UiSettingsJson), db);
        return NoContent();
    }
}

public sealed record UpdateMyUserSettingsRequest(string Theme, string UiSettingsJson);

public sealed class UpdateMyUserSettingsValidator : AbstractValidator<UpdateMyUserSettingsRequest>
{
    public UpdateMyUserSettingsValidator()
    {
        RuleFor(x => x.Theme).NotEmpty().MaximumLength(50);
        RuleFor(x => x.UiSettingsJson).NotEmpty().MaximumLength(10000);
    }
}
