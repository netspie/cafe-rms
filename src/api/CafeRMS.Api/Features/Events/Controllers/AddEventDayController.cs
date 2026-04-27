using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Events.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Events.Controllers;

[ApiController]
public sealed class AddEventDayController : ControllerBase
{
    [HttpPost("/api/events/{id:guid}/days")]
    [Authorize(Policy = Permissions.EventsManage)]
    public async Task<AddEventDayResponse> Handle(
        [FromRoute] Guid id,
        [FromBody] AddEventDayRequest request,
        [FromServices] AppDbContext db)
    {
        var result = await AddEventDay.Execute(id, request.Date, db);
        return new AddEventDayResponse(result.Id);
    }
}

public sealed record AddEventDayRequest(DateOnly Date);
public sealed record AddEventDayResponse(Guid Id);

public sealed class AddEventDayValidator : AbstractValidator<AddEventDayRequest>
{
    public AddEventDayValidator()
    {
        RuleFor(x => x.Date).NotEqual(default(DateOnly));
    }
}
