using CafeRMS.Api.Features.Allergens.UseCases;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Allergens.Controllers;

[ApiController]
public sealed class AddAllergenController : ControllerBase
{
    [HttpPost("/api/allergens")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<AddAllergenResponse> Handle(
        [FromBody] AddAllergenRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new AddAllergen.Command(db.CurrentCompanyId, request.Name);
        var result = await AddAllergen.Execute(command, db);
        return new AddAllergenResponse(result.Id);
    }
}

public sealed record AddAllergenRequest(string Name);

public sealed record AddAllergenResponse(Guid Id);

public sealed class AddAllergenValidator : AbstractValidator<AddAllergenRequest>
{
    public AddAllergenValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
