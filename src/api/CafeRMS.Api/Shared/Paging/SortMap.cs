using System.Linq.Expressions;

namespace CafeRMS.Api.Shared.Paging;

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
