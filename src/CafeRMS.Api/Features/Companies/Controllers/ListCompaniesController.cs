using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Companies.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Companies.Controllers;

[ApiController]
public sealed class ListCompaniesController : ControllerBase
{
    [HttpGet("/api/companies")]
    [Authorize(Policy = Policies.RequireSuperAdmin)]
    public async Task<IReadOnlyList<ListCompanies.Item>> Handle(
        [FromServices] AppDbContext db) =>
        await ListCompanies.Execute(db);
}
