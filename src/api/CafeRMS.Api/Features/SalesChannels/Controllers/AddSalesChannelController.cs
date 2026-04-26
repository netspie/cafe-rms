using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.SalesChannels.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.SalesChannels.Controllers;

[ApiController]
public sealed class AddSalesChannelController : ControllerBase
{
    [HttpPost("/api/sales-channels")]
    [Authorize(Policy = Permissions.SalesChannelsManage)]
    public async Task<AddSalesChannelResponse> Handle(
        [FromBody] AddSalesChannelRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new AddSalesChannel.Command(db.CurrentCompanyId, request.Name, request.IsTakeout);
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
