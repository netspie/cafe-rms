using System.Reflection;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Shared;

// Resource-based authorization for Guest-owned resources (Order, Favorite, UserSettings,
// LoyaltyPointLog, …) — declarative ownership check that runs before the action body.
//
// Usage on a controller action:
//   [HttpDelete("/api/my/favorites/{id:guid}")]
//   [Authorize(Policy = Policies.RequireGuest)]
//   [ResourceOwner<Favorite>("id", nameof(Favorite.UserId))]
//   public async Task<IActionResult> Handle([FromRoute] Guid id, ...) { ... }
//
// What the filter does:
//   1. Pulls the route argument with the given name and parses it as a Guid.
//   2. Fetches the entity via AppDbContext.Set<TEntity>().FindAsync(...).
//      Soft-delete + ICompanyOwned global filters apply as usual: the entity is
//      considered "not found" if it's hidden from the caller. (Add IgnoreQueryFilters
//      to the use case if you need to read past those — this attribute does not.)
//   3. Reads the named property (must be Guid or Guid?) and compares to User.UserId.
//   4. Throws NotFoundException if the entity doesn't exist, ForbiddenException if
//      the caller is not the owner. Otherwise the action proceeds.
//
// 404 over 403 on missing entity is intentional — it avoids leaking the existence of
// resources owned by other users via id enumeration.
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class ResourceOwnerAttribute<TEntity> : Attribute, IAsyncActionFilter
    where TEntity : class
{
    public string RouteArg { get; }
    public string OwnerPropertyName { get; }

    public ResourceOwnerAttribute(string routeArg, string ownerPropertyName)
    {
        RouteArg = routeArg;
        OwnerPropertyName = ownerPropertyName;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.RouteData.Values.TryGetValue(RouteArg, out var raw) ||
            !Guid.TryParse(raw?.ToString(), out var resourceId))
            throw new DomainException($"Route argument '{RouteArg}' is missing or not a Guid.");

        var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        var entity = await db.Set<TEntity>().FindAsync([resourceId], context.HttpContext.RequestAborted)
            ?? throw new NotFoundException($"{typeof(TEntity).Name} not found.");

        var prop = typeof(TEntity).GetProperty(OwnerPropertyName, BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException(
                $"Property '{OwnerPropertyName}' not found on {typeof(TEntity).Name}.");

        var ownerIdValue = prop.GetValue(entity);
        var ownerId = ownerIdValue switch
        {
            Guid guid => guid,
            null => Guid.Empty,
            _ => throw new InvalidOperationException(
                $"Property '{OwnerPropertyName}' on {typeof(TEntity).Name} is not a Guid.")
        };

        if (ownerId != context.HttpContext.User.UserId)
            throw new NotFoundException($"{typeof(TEntity).Name} not found.");

        await next();
    }
}
