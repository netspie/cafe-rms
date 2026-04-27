using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Loyalty.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Loyalty.Controllers;

[ApiController]
public sealed class AddLoyaltyAdjustmentController : ControllerBase
{
    [HttpPost("/api/loyalty/entries")]
    [Authorize(Policy = Permissions.LoyaltyManage)]
    public async Task<AddLoyaltyAdjustmentResponse> Handle(
        [FromBody] AddLoyaltyAdjustmentRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new AddLoyaltyAdjustment.Command(db.CurrentCompanyId, request.UserId, request.Points, request.Reason);
        var result = await AddLoyaltyAdjustment.Execute(command, db);
        return new AddLoyaltyAdjustmentResponse(result.Id);
    }
}

public sealed record AddLoyaltyAdjustmentRequest(Guid UserId, int Points, string Reason);
public sealed record AddLoyaltyAdjustmentResponse(Guid Id);

public sealed class AddLoyaltyAdjustmentValidator : AbstractValidator<AddLoyaltyAdjustmentRequest>
{
    public AddLoyaltyAdjustmentValidator()
    {
        RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
