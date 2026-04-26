using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.PriceGroups.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.PriceGroups.Controllers;

[ApiController]
public sealed class AddPriceGroupController : ControllerBase
{
    [HttpPost("/api/price-groups")]
    [Authorize(Policy = Permissions.PricingManage)]
    public async Task<AddPriceGroupResponse> Handle(
        [FromBody] AddPriceGroupRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new AddPriceGroup.Command(db.CurrentCompanyId, request.Name);
        var result = await AddPriceGroup.Execute(command, db);
        return new AddPriceGroupResponse(result.Id);
    }
}

public sealed record AddPriceGroupRequest(string Name);

public sealed record AddPriceGroupResponse(Guid Id);

public sealed class AddPriceGroupValidator : AbstractValidator<AddPriceGroupRequest>
{
    public AddPriceGroupValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
