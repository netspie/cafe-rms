using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Loyalty.UseCases;

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


public static class AddLoyaltyAdjustment
{
    public sealed record Command(Guid CompanyId, Guid UserId, int Points, string Reason);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        if (command.CompanyId == Guid.Empty)
            throw new ForbiddenException("A company context is required for a loyalty adjustment.");

        if (command.Points == 0)
            throw new DomainException("Points must be non-zero.");

        var userExists = await db.Users.AnyAsync(x => x.Id == command.UserId);
        if (!userExists)
            throw new NotFoundException("User not found.");

        var entry = LoyaltyPointLog.Create(command.UserId, command.Points, command.CompanyId, command.Reason);
        db.LoyaltyPointLogs.Add(entry);
        await db.SaveChangesAsync();
        return new Result(entry.Id);
    }
}
