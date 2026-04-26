using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.PromotionCodes.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.PromotionCodes.Controllers;

[ApiController]
public sealed class AddPromotionCodeController : ControllerBase
{
    [HttpPost("/api/promotion-codes")]
    [Authorize(Policy = Permissions.PromotionsManage)]
    public async Task<AddPromotionCodeResponse> Handle(
        [FromBody] AddPromotionCodeRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new AddPromotionCode.Command(
            db.CurrentCompanyId,
            request.Code,
            request.DiscountPercentage,
            request.ValidFrom,
            request.ValidUntil,
            request.MaxUses);
        var result = await AddPromotionCode.Execute(command, db);
        return new AddPromotionCodeResponse(result.Id);
    }
}

public sealed record AddPromotionCodeRequest(
    string Code,
    decimal DiscountPercentage,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidUntil,
    int? MaxUses);

public sealed record AddPromotionCodeResponse(Guid Id);

public sealed class AddPromotionCodeValidator : AbstractValidator<AddPromotionCodeRequest>
{
    public AddPromotionCodeValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DiscountPercentage).InclusiveBetween(0m, 100m);
        RuleFor(x => x.MaxUses).GreaterThan(0).When(x => x.MaxUses.HasValue);
    }
}
