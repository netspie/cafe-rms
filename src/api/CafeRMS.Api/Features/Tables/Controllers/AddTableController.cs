using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Tables.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Tables.Controllers;

[ApiController]
public sealed class AddTableController : ControllerBase
{
    [HttpPost("/api/tables")]
    [Authorize(Policy = Permissions.TablesManage)]
    public async Task<AddTableResponse> Handle(
        [FromBody] AddTableRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new AddTable.Command(db.CurrentCompanyId, request.Name);
        var result = await AddTable.Execute(command, db);
        return new AddTableResponse(result.Id);
    }
}

public sealed record AddTableRequest(string Name);

public sealed record AddTableResponse(Guid Id);

public sealed class AddTableValidator : AbstractValidator<AddTableRequest>
{
    public AddTableValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
