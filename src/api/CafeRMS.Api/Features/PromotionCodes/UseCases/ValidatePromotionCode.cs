using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.PromotionCodes.UseCases;

[ApiController]
public sealed class ValidatePromotionCodeController : ControllerBase
{
    [HttpPost("/api/promotion-codes/validate")]
    [Authorize] // any authenticated user — used by mobile app pre-checkout
    public async Task<ValidatePromotionCode.Result> Handle(
        [FromBody] ValidatePromotionCodeRequest request,
        [FromServices] AppDbContext db) =>
        await ValidatePromotionCode.Execute(request.Code, DateTimeOffset.UtcNow, db);
}

public sealed record ValidatePromotionCodeRequest(string Code);

public sealed class ValidatePromotionCodeValidator : AbstractValidator<ValidatePromotionCodeRequest>
{
    public ValidatePromotionCodeValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
    }
}


public static class ValidatePromotionCode
{
    // Reasons mirror the use-cases.md spec; mobile app surfaces these to the customer.
    public const string ReasonNotFound = "not-found";
    public const string ReasonNotYetActive = "not-yet-active";
    public const string ReasonExpired = "expired";
    public const string ReasonMaxUsesReached = "max-uses-reached";

    public sealed record Result(bool Valid, decimal? DiscountPercentage, string? Reason);

    public static async Task<Result> Execute(string code, DateTimeOffset now, AppDbContext db)
    {
        var promo = await db.PromotionCodes.FirstOrDefaultAsync(x => x.Code == code);
        if (promo is null)
            return new Result(false, null, ReasonNotFound);

        if (promo.ValidFrom is { } from && now < from)
            return new Result(false, null, ReasonNotYetActive);

        if (promo.ValidUntil is { } until && now > until)
            return new Result(false, null, ReasonExpired);

        if (promo.MaxUses is { } max && promo.UsesCount >= max)
            return new Result(false, null, ReasonMaxUsesReached);

        return new Result(true, promo.DiscountPercentage, null);
    }
}
