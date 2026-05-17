using System.Linq.Expressions;
using System.Reflection;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Shared.Paging;

public static class PagingExtensions
{
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> query,
        string? sortExpression,
        SortMap<T> allowedFields,
        string? defaultSortExpression = null)
    {
        sortExpression = string.IsNullOrWhiteSpace(sortExpression) ? defaultSortExpression : sortExpression;
        if (string.IsNullOrWhiteSpace(sortExpression))
            return query;

        var fields = sortExpression.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        IOrderedQueryable<T>? orderedQuery = null;

        foreach (var field in fields)
        {
            var isDescending = field.StartsWith('-');
            var fieldName = isDescending ? field[1..] : field;

            if (!allowedFields.TryGet(fieldName, out var propertySelector))
                throw new DomainException($"Unsupported sort field: {fieldName}");

            orderedQuery = ApplyOrdering(
                sourceQuery: orderedQuery is null ? query : orderedQuery,
                propertySelector: propertySelector,
                isDescending: isDescending,
                isSecondarySort: orderedQuery is not null);
        }

        return orderedQuery ?? query;
    }

    public static async Task<PagedResult<TResult>> ToPagedResultAsync<TResult>(
        this IQueryable<TResult> query,
        PagedQuery pageRequest,
        CancellationToken ct = default)
    {
        var (page, pageSize) = NormalizePageRequest(pageRequest);
        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<TResult>(items, page, pageSize, totalCount);
    }

    private static (int page, int pageSize) NormalizePageRequest(PagedQuery pageRequest)
    {
        var page = Math.Max(1, pageRequest.Page);
        var requestedPageSize = pageRequest.PageSize <= 0 ? PagedQuery.DefaultPageSize : pageRequest.PageSize;
        var pageSize = Math.Min(requestedPageSize, PagedQuery.MaxPageSize);
        return (page, pageSize);
    }

    private static IOrderedQueryable<T> ApplyOrdering<T>(
        IQueryable<T> sourceQuery,
        LambdaExpression propertySelector,
        bool isDescending,
        bool isSecondarySort)
    {
        var orderingMethodName = (isSecondarySort, isDescending) switch
        {
            (false, false) => nameof(Queryable.OrderBy),
            (false, true) => nameof(Queryable.OrderByDescending),
            (true, false) => nameof(Queryable.ThenBy),
            (true, true) => nameof(Queryable.ThenByDescending),
        };

        var orderingMethod = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(x => x.Name == orderingMethodName && x.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), propertySelector.ReturnType);

        return (IOrderedQueryable<T>)orderingMethod.Invoke(null, [sourceQuery, propertySelector])!;
    }
}
