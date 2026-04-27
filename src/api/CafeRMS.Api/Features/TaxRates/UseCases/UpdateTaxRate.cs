using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.TaxRates.UseCases;

[ApiController]
public sealed class UpdateTaxRateController : ControllerBase
{
    [HttpPut("/api/tax-rates/{id:guid}")]
    [Authorize(Policy = Permissions.TaxRatesManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdateTaxRateRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new UpdateTaxRate.Command(id, request.Name, request.Description, request.Rate);
        await UpdateTaxRate.Execute(command, db);
        return NoContent();
    }
}

public sealed record UpdateTaxRateRequest(string Name, string Description, decimal Rate);

public sealed class UpdateTaxRateValidator : AbstractValidator<UpdateTaxRateRequest>
{
    public UpdateTaxRateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).NotNull().MaximumLength(500);
        RuleFor(x => x.Rate).InclusiveBetween(0m, 100m);
    }
}


public static class UpdateTaxRate
{
    public sealed record Command(Guid Id, string Name, string Description, decimal Rate);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var taxRate = await db.TaxRates.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Tax rate not found.");

        var nameTaken = await db.TaxRates.AnyAsync(x => x.Name == command.Name && x.Id != command.Id);
        if (nameTaken)
            throw new ConflictException($"A tax rate named '{command.Name}' already exists.");

        taxRate.Update(command.Name, command.Description, command.Rate);
        await db.SaveChangesAsync();
    }
}
