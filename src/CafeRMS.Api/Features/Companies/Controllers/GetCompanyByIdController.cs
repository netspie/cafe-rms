using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Companies.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Companies.Controllers;

[ApiController]
public sealed class GetCompanyByIdController : ControllerBase
{
    [HttpGet("/api/companies/{id:guid}")]
    public async Task<GetCompanyById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        // SuperAdmin sees any; Staff sees only their own; Guest never sees a company by id.
        var isSuperAdmin = User.AccountType == AccountType.SuperAdmin;
        var ownsThisCompany = User.CompanyId == id;
        if (!isSuperAdmin && !ownsThisCompany)
            throw new ForbiddenException("You do not have access to this company.");

        return await GetCompanyById.Execute(id, db);
    }
}
