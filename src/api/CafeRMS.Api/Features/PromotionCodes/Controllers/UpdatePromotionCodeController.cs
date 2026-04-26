using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.PromotionCodes.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.PromotionCodes.Controllers;

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
