using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.PromotionCodes.UseCases;

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


public static class AddPromotionCode
{
    public sealed record Command(
        Guid CompanyId,
        string Code,
        decimal DiscountPercentage,
        DateTimeOffset? ValidFrom,
        DateTimeOffset? ValidUntil,
        int? MaxUses);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        if (command.CompanyId == Guid.Empty)
            throw new ForbiddenException("A company context is required to create a promotion code.");

        if (command.ValidFrom is DateTimeOffset from && command.ValidUntil is DateTimeOffset until && from > until)
            throw new DomainException("ValidFrom must be earlier than ValidUntil.");

        var codeTaken = await db.PromotionCodes.AnyAsync(x => x.Code == command.Code);
        if (codeTaken)
            throw new ConflictException($"A promotion code '{command.Code}' already exists.");

        var promo = PromotionCode.Create(
            command.Code,
            command.DiscountPercentage,
            command.CompanyId,
            command.ValidFrom,
            command.ValidUntil,
            command.MaxUses);

        db.PromotionCodes.Add(promo);
        await db.SaveChangesAsync();
        return new Result(promo.Id);
    }
}
