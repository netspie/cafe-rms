using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Outlets.UseCases;

[ApiController]
public sealed class GetOutletByIdController : ControllerBase
{
    [HttpGet("/api/outlets/{id:guid}")]
    [Authorize]
    public async Task<GetOutletById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetOutletById.Execute(id, db);
}


public static class GetOutletById
{
    public sealed record Result(
        Guid Id,
        string DisplayName,
        string StreetAddress,
        string Phone,
        string TimeZone,
        string Currency,
        string? LogoUrl,
        DateTimeOffset CreatedAt,
        DateTimeOffset? UpdatedAt);

    public static async Task<Result> Execute(Guid id, AppDbContext db)
    {
        var outlet = await db.Outlets
            .Where(x => x.Id == id)
            .Select(x => new Result(
                x.Id, x.DisplayName, x.StreetAddress, x.Phone, x.TimeZone,
                x.Currency.ToString(), x.LogoUrl, x.CreatedAt, x.UpdatedAt))
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Outlet not found.");

        return outlet;
    }
}
