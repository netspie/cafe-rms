using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Companies.UseCases;

[ApiController]
public sealed class ListCompaniesController : ControllerBase
{
    [HttpGet("/api/companies")]
    [Authorize(Policy = Policies.RequireSuperAdmin)]
    public async Task<IReadOnlyList<ListCompanies.Item>> Handle(
        [FromServices] AppDbContext db) =>
        await ListCompanies.Execute(db);
}


public static class ListCompanies
{
    public sealed record Item(
        Guid Id,
        string LegalName,
        string TaxId,
        string BillingEmail,
        bool IsPublic,
        DateTimeOffset CreatedAt);

    public static async Task<IReadOnlyList<Item>> Execute(AppDbContext db) =>
        await db.Companies
            .OrderBy(x => x.LegalName)
            .Select(x => new Item(x.Id, x.LegalName, x.TaxId, x.BillingEmail, x.IsPublic, x.CreatedAt))
            .ToListAsync();
}
