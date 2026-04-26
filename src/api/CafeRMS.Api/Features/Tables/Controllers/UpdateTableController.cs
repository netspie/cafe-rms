using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Tables.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Tables.Controllers;

[ApiController]
public sealed class UpdateTableController : ControllerBase
{
    [HttpPut("/api/tables/{id:guid}")]
    [Authorize(Policy = Permissions.TablesManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdateTableRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new UpdateTable.Command(id, request.Name);
        await UpdateTable.Execute(command, db);
        return NoContent();
    }
}

public sealed record UpdateTableRequest(string Name);

public sealed class UpdateTableValidator : AbstractValidator<UpdateTableRequest>
{
    public UpdateTableValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
