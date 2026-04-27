using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Allergens.UseCases;

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


public static class UpdateAllergen
{
    public sealed record Command(Guid Id, string Name);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var allergen = await db.Allergens.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Allergen not found.");

        var nameTaken = await db.Allergens.AnyAsync(x => x.Name == command.Name && x.Id != command.Id);
        if (nameTaken)
            throw new ConflictException($"An allergen named '{command.Name}' already exists.");

        allergen.Update(command.Name);
        await db.SaveChangesAsync();
    }
}
