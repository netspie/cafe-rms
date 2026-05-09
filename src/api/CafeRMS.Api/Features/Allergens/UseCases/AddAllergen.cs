using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Allergens.UseCases;

[ApiController]
public sealed class AddAllergenController : ControllerBase
{
    [HttpPost("/api/allergens")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<AddAllergenResponse> Handle(
        [FromBody] AddAllergenRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new AddAllergen.Command(request.Name);
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


public static class AddAllergen
{
    public sealed record Command(string Name);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        var nameTaken = await db.Allergens.AnyAsync(x => x.Name == command.Name);
        if (nameTaken)
            throw new ConflictException($"An allergen named '{command.Name}' already exists.");

        var allergen = Allergen.Create(command.Name);
        db.Allergens.Add(allergen);
        await db.SaveChangesAsync();
        return new Result(allergen.Id);
    }
}
