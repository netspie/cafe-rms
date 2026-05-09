using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.TaxRates.UseCases;

[ApiController]
public sealed class AddTaxRateController : ControllerBase
{
    [HttpPost("/api/tax-rates")]
    [Authorize(Policy = Permissions.TaxRatesManage)]
    public async Task<AddTaxRateResponse> Handle(
        [FromBody] AddTaxRateRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new AddTaxRate.Command(request.Name, request.Description, request.Rate);
        var result = await AddTaxRate.Execute(command, db);
        return new AddTaxRateResponse(result.Id);
    }
}

public sealed record AddTaxRateRequest(string Name, string Description, decimal Rate);

public sealed record AddTaxRateResponse(Guid Id);

public sealed class AddTaxRateValidator : AbstractValidator<AddTaxRateRequest>
{
    public AddTaxRateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).NotNull().MaximumLength(500);
        RuleFor(x => x.Rate).InclusiveBetween(0m, 100m);
    }
}


public static class AddTaxRate
{
    public sealed record Command(string Name, string Description, decimal Rate);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        var nameTaken = await db.TaxRates.AnyAsync(x => x.Name == command.Name);
        if (nameTaken)
            throw new ConflictException($"A tax rate named '{command.Name}' already exists.");

        var taxRate = TaxRate.Create(command.Name, command.Description, command.Rate);
        db.TaxRates.Add(taxRate);
        await db.SaveChangesAsync();
        return new Result(taxRate.Id);
    }
}
