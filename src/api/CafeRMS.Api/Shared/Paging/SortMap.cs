using System.Linq.Expressions;

namespace CafeRMS.Api.Shared.Paging;

// Allow-list of fields a caller may sort by. Each entry maps a URL-facing name
// (e.g. "createdAt") to a typed property selector (e.g. p => p.CreatedAt).
// Using LambdaExpression (not Expression<Func<T, object>>) keeps the property's
// native type, so EF doesn't have to translate a Convert() wrapper in OrderBy.
public sealed class SortMap<T>
{
    private readonly Dictionary<string, LambdaExpression> _sortableFields = new(StringComparer.OrdinalIgnoreCase);

    public SortMap<T> Add<TProperty>(string fieldName, Expression<Func<T, TProperty>> propertySelector)
    {
        _sortableFields[fieldName] = propertySelector;
        return this;
    }

    public bool TryGet(string fieldName, out LambdaExpression propertySelector) =>
        _sortableFields.TryGetValue(fieldName, out propertySelector!);
}
