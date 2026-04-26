using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.SalesChannels.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.SalesChannels.Controllers;

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
