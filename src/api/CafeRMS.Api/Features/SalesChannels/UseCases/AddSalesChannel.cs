using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.SalesChannels.UseCases;

[ApiController]
public sealed class AddSalesChannelController : ControllerBase
{
    [HttpPost("/api/sales-channels")]
    [Authorize(Policy = Permissions.SalesChannelsManage)]
    public async Task<AddSalesChannelResponse> Handle(
        [FromBody] AddSalesChannelRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new AddSalesChannel.Command(request.Name, request.IsTakeout);
        var result = await AddSalesChannel.Execute(command, db);
        return new AddSalesChannelResponse(result.Id);
    }
}

public sealed record AddSalesChannelRequest(string Name, bool IsTakeout);

public sealed record AddSalesChannelResponse(Guid Id);

public sealed class AddSalesChannelValidator : AbstractValidator<AddSalesChannelRequest>
{
    public AddSalesChannelValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}


public static class AddSalesChannel
{
    public sealed record Command(string Name, bool IsTakeout);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        var nameTaken = await db.SalesChannels.AnyAsync(x => x.Name == command.Name);
        if (nameTaken)
            throw new ConflictException($"A sales channel named '{command.Name}' already exists.");

        var channel = SalesChannel.Create(command.Name, command.IsTakeout);
        db.SalesChannels.Add(channel);
        await db.SaveChangesAsync();
        return new Result(channel.Id);
    }
}
