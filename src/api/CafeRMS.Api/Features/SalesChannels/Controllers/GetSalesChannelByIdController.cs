using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.SalesChannels.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.SalesChannels.Controllers;

[ApiController]
public sealed class GetSalesChannelByIdController : ControllerBase
{
    [HttpGet("/api/sales-channels/{id:guid}")]
    [Authorize(Policy = Permissions.SalesChannelsManage)]
    public async Task<GetSalesChannelById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetSalesChannelById.Execute(id, db);
}
