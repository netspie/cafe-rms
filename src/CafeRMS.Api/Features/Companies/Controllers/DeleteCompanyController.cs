using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Companies.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Companies.Controllers;

[ApiController]
public sealed class DeleteCompanyController : ControllerBase
{
    [HttpDelete("/api/companies/{id:guid}")]
    [Authorize(Policy = Policies.RequireSuperAdmin)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await DeleteCompany.Execute(id, db);
        return NoContent();
    }
}
