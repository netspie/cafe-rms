using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.SalesChannels.UseCases;

[ApiController]
public sealed class UpdateSalesChannelController : ControllerBase
{
    [HttpPut("/api/sales-channels/{id:guid}")]
    [Authorize(Policy = Permissions.SalesChannelsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdateSalesChannelRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new UpdateSalesChannel.Command(id, request.Name, request.IsTakeout);
        await UpdateSalesChannel.Execute(command, db);
        return NoContent();
    }
}

public sealed record UpdateSalesChannelRequest(string Name, bool IsTakeout);

public sealed class UpdateSalesChannelValidator : AbstractValidator<UpdateSalesChannelRequest>
{
    public UpdateSalesChannelValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}


public static class UpdateSalesChannel
{
    public sealed record Command(Guid Id, string Name, bool IsTakeout);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var channel = await db.SalesChannels.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Sales channel not found.");

        var nameTaken = await db.SalesChannels.AnyAsync(x => x.Name == command.Name && x.Id != command.Id);
        if (nameTaken)
            throw new ConflictException($"A sales channel named '{command.Name}' already exists.");

        channel.Update(command.Name, command.IsTakeout);
        await db.SaveChangesAsync();
    }
}
