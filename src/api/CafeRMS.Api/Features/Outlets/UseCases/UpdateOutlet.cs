using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Outlets.UseCases;

[ApiController]
public sealed class UpdateOutletController : ControllerBase
{
    [HttpPut("/api/outlets/{id:guid}")]
    [Authorize(Policy = Permissions.OutletManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdateOutletRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new UpdateOutlet.Command(
            id,
            request.DisplayName,
            request.StreetAddress,
            request.Phone,
            request.TimeZone,
            request.Currency,
            request.LegalName,
            request.TaxId,
            request.InvoicingAddress,
            request.BillingEmail,
            request.BillingPhone,
            request.LogoUrl,
            request.DefaultPriceGroupId,
            request.DefaultProductListId);
        await UpdateOutlet.Execute(command, db);
        return NoContent();
    }
}

public sealed record UpdateOutletRequest(
    string DisplayName,
    string StreetAddress,
    string Phone,
    string TimeZone,
    Currency Currency,
    string LegalName,
    string TaxId,
    string InvoicingAddress,
    string BillingEmail,
    string BillingPhone,
    string? LogoUrl,
    Guid? DefaultPriceGroupId,
    Guid? DefaultProductListId);

public sealed class UpdateOutletValidator : AbstractValidator<UpdateOutletRequest>
{
    public UpdateOutletValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StreetAddress).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(50);
        RuleFor(x => x.TimeZone).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LogoUrl).MaximumLength(500);
        RuleFor(x => x.LegalName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TaxId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.InvoicingAddress).NotEmpty().MaximumLength(500);
        RuleFor(x => x.BillingEmail).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.BillingPhone).NotEmpty().MaximumLength(50);
    }
}


public static class UpdateOutlet
{
    public sealed record Command(
        Guid Id,
        string DisplayName,
        string StreetAddress,
        string Phone,
        string TimeZone,
        Currency Currency,
        string LegalName,
        string TaxId,
        string InvoicingAddress,
        string BillingEmail,
        string BillingPhone,
        string? LogoUrl,
        Guid? DefaultPriceGroupId,
        Guid? DefaultProductListId);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var outlet = await db.Outlets.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Outlet not found.");

        outlet.Update(
            command.DisplayName,
            command.StreetAddress,
            command.Phone,
            command.TimeZone,
            command.Currency,
            command.LegalName,
            command.TaxId,
            command.InvoicingAddress,
            command.BillingEmail,
            command.BillingPhone,
            command.LogoUrl);
        outlet.SetDefaultMenu(command.DefaultPriceGroupId, command.DefaultProductListId);
        await db.SaveChangesAsync();
    }
}
