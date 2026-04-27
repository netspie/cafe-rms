using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.PromotionCodes.UseCases;

[ApiController]
public sealed class UpdatePromotionCodeController : ControllerBase
{
    [HttpPut("/api/promotion-codes/{id:guid}")]
    [Authorize(Policy = Permissions.PromotionsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdatePromotionCodeRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new UpdatePromotionCode.Command(
            id,
            request.Code,
            request.DiscountPercentage,
            request.ValidFrom,
            request.ValidUntil,
            request.MaxUses);
        await UpdatePromotionCode.Execute(command, db);
        return NoContent();
    }
}

public sealed record UpdatePromotionCodeRequest(
    string Code,
    decimal DiscountPercentage,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidUntil,
    int? MaxUses);

public sealed class UpdatePromotionCodeValidator : AbstractValidator<UpdatePromotionCodeRequest>
{
    public UpdatePromotionCodeValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DiscountPercentage).InclusiveBetween(0m, 100m);
        RuleFor(x => x.MaxUses).GreaterThan(0).When(x => x.MaxUses.HasValue);
    }
}


public static class UpdatePromotionCode
{
    public sealed record Command(
        Guid Id,
        string Code,
        decimal DiscountPercentage,
        DateTimeOffset? ValidFrom,
        DateTimeOffset? ValidUntil,
        int? MaxUses);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var promo = await db.PromotionCodes.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Promotion code not found.");

        if (command.ValidFrom is { } from && command.ValidUntil is { } until && from > until)
            throw new DomainException("ValidFrom must be earlier than ValidUntil.");

        var codeTaken = await db.PromotionCodes.AnyAsync(x => x.Code == command.Code && x.Id != command.Id);
        if (codeTaken)
            throw new ConflictException($"A promotion code '{command.Code}' already exists.");

        promo.Update(command.Code, command.DiscountPercentage, command.ValidFrom, command.ValidUntil, command.MaxUses);
        await db.SaveChangesAsync();
    }
}
