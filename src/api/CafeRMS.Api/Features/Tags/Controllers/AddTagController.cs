using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Tags.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Tags.Controllers;

[ApiController]
public sealed class AddTagController : ControllerBase
{
    [HttpPost("/api/tags")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<AddTagResponse> Handle(
        [FromBody] AddTagRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new AddTag.Command(db.CurrentCompanyId, request.Name, request.ImageUrl);
        var result = await AddTag.Execute(command, db);
        return new AddTagResponse(result.Id);
    }
}

public sealed record AddTagRequest(string Name, string? ImageUrl);

public sealed record AddTagResponse(Guid Id);

public sealed class AddTagValidator : AbstractValidator<AddTagRequest>
{
    public AddTagValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ImageUrl).MaximumLength(500);
    }
}
