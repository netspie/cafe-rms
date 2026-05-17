using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
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
        var command = new AddLoyaltyAdjustment.Command(request.UserId, request.Points, request.Reason, User.UserId);
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
        RuleFor(x => x.Points).NotEqual(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}


public static class AddLoyaltyAdjustment
{
    public sealed record Command(Guid UserId, int Points, string Reason, Guid ActorUserId);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        var userExists = await db.Users.AnyAsync(x => x.Id == command.UserId);
        if (!userExists)
            throw new NotFoundException("User not found.");

        if (command.Points < 0)
        {
            var balance = await db.Database
                .SqlQuery<int>($"SELECT func_loyalty_balance({command.UserId}) AS \"Value\"")
                .SingleAsync();
            if (balance < -command.Points)
                throw new DomainException($"Insufficient loyalty balance ({balance} available).");
        }

        var id = Guid.NewGuid();
        await db.Database.ExecuteSqlAsync(
            $"CALL proc_adjust_loyalty_points({id}, {command.UserId}, {command.Points}, {command.Reason}, {command.ActorUserId})");

        return new Result(id);
    }
}
