using CafeRMS.Api.Features.PromotionCodes.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.PromotionCodes.Controllers;

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
