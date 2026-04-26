using CafeRMS.Api.Features.Allergens.UseCases;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Allergens.Controllers;

[ApiController]
public sealed class UpdateAllergenController : ControllerBase
{
    [HttpPut("/api/allergens/{id:guid}")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdateAllergenRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new UpdateAllergen.Command(id, request.Name);
        await UpdateAllergen.Execute(command, db);
        return NoContent();
    }
}

public sealed record UpdateAllergenRequest(string Name);

public sealed class UpdateAllergenValidator : AbstractValidator<UpdateAllergenRequest>
{
    public UpdateAllergenValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
