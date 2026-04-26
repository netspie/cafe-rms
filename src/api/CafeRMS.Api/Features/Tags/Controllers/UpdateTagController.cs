using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Tags.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Tags.Controllers;

[ApiController]
public sealed class UpdateTagController : ControllerBase
{
    [HttpPut("/api/tags/{id:guid}")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdateTagRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new UpdateTag.Command(id, request.Name, request.ImageUrl);
        await UpdateTag.Execute(command, db);
        return NoContent();
    }
}

public sealed record UpdateTagRequest(string Name, string? ImageUrl);

public sealed class UpdateTagValidator : AbstractValidator<UpdateTagRequest>
{
    public UpdateTagValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ImageUrl).MaximumLength(500);
    }
}
