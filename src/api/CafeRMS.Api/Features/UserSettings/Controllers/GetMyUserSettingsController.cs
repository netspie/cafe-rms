using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.UserSettings.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.UserSettings.Controllers;

[ApiController]
public sealed class GetMyUserSettingsController : ControllerBase
{
    [HttpGet("/api/my/settings")]
    [Authorize(Policy = Policies.RequireGuest)]
    public Task<GetMyUserSettings.Result> Handle(
        [FromServices] AppDbContext db) =>
        GetMyUserSettings.Execute(User.UserId, db);
}
