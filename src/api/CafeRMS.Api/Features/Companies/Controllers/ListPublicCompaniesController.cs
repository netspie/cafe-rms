using CafeRMS.Api.Features.Companies.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Companies.Controllers;

[ApiController]
public sealed class ListPublicCompaniesController : ControllerBase
{
    [HttpGet("/api/companies/public")]
    [AllowAnonymous]
    public async Task<IReadOnlyList<ListPublicCompanies.Item>> Handle(
        [FromServices] AppDbContext db) =>
        await ListPublicCompanies.Execute(db);
}
