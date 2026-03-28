using System.Linq.Expressions;
using System.Reflection;
using Domain.Attributes;
using Domain.Dto.RegistryDto;
using Domain.Helpers;

namespace Dal.RegistryRepositories;

public abstract class Orderer<TModel, TSearchModel> : IRegistryRepositoryOrderer<TModel, TSearchModel>
    where TSearchModel : IWithSearchParameters
{
    public IQueryable<TModel> Order(IQueryable<TModel> queryable, TSearchModel searchModel)
    {
        if (searchModel.SearchParameters == null! || string.IsNullOrEmpty(searchModel.SearchParameters.OrderBy))
        {
            return queryable;
        }
        var orderPropertyName = searchModel.SearchParameters.OrderBy;

        var selectorDictionary = GetDictionary();
        var enumSelectorDictionary = GetEnumDictionaryOrder();

        if (!searchModel.SearchParameters.OrderAsc.HasValue
            || (!enumSelectorDictionary.ContainsKey(orderPropertyName) &&
                !selectorDictionary.ContainsKey(orderPropertyName)))
        {
            return queryable;
        }
        var orderedQuery = GetOrderedQuery(queryable, selectorDictionary, enumSelectorDictionary, searchModel);

        if (!string.IsNullOrEmpty(searchModel.SearchParameters.ThenBy))
        {
            orderedQuery = GetThenOrderedQuery(orderedQuery, selectorDictionary, enumSelectorDictionary, searchModel);
        }

        return orderedQuery.ThenBy(DefaultOrderExpression);

    }

    protected virtual Dictionary<string, Expression<Func<TModel, object>>> GetDictionary()
    {
        var properties = typeof(TModel)
            .GetProperties()
            .Where(property => property.GetCustomAttribute(typeof(SortFieldAttribute)) != null
                               && !property.PropertyType.IsEnum)
            .ToArray();

        var dictionary = new Dictionary<string, Expression<Func<TModel, object>>>();

        foreach (var property in properties)
        {
            var attribute = property.GetCustomAttribute<SortFieldAttribute>();

            var parameter = Expression.Parameter(typeof(TModel));
            var propertyExpression = Expression.Property(parameter, property);
            var sortExpression =
                Expression.Lambda<Func<TModel, object>>(Expression.Convert(propertyExpression, typeof(object)),
                    parameter);
            dictionary.Add(attribute!.FieldName, sortExpression);
        }

        return dictionary;
    }

    protected virtual Dictionary<string, Expression<Func<TModel, int>>> GetEnumDictionaryOrder()
    {
        var properties = typeof(TModel)
            .GetProperties()
            .Where(property => property.GetCustomAttribute(typeof(SortFieldAttribute)) != null &&
                               property.PropertyType.IsEnum)
            .ToArray();
        var result = new Dictionary<string, Expression<Func<TModel, int>>>();
        foreach (var property in properties)
        {
            var attribute = property.GetCustomAttribute<SortFieldAttribute>();
            var parameter = Expression.Parameter(typeof(TModel));
            var propertyExpression = Expression.Property(parameter, property);
            var sortExpression =
                Expression.Lambda(Expression.Convert(propertyExpression, property.PropertyType), parameter);

            var enums = Enum.GetValues(property.PropertyType).Cast<Enum>();
            var body = enums
                .OrderBy(value => value.GetSortOrder() ?? value.GetDescription())
                .Select((value, ordinal) => new { value, ordinal })
                .Reverse()
                .Aggregate((Expression)null!, (next, item) => next == null!
                    ? Expression.Constant(item.ordinal)
                    : Expression.Condition(
                        Expression.Equal(sortExpression.Body, Expression.Constant(item.value)),
                        Expression.Constant(item.ordinal),
                        next));


            var lambda = Expression.Lambda<Func<TModel, int>>(body, parameter);
            result.Add(attribute!.FieldName, lambda);
        }

        return result;
    }

    private IOrderedQueryable<TModel> GetOrderedQuery(IQueryable<TModel> queryable,
        IReadOnlyDictionary<string, Expression<Func<TModel, object>>> selectorDict,
        IReadOnlyDictionary<string, Expression<Func<TModel, int>>> enumSelectorDict,
        TSearchModel searchModel)
    {
        if (enumSelectorDict.TryGetValue(searchModel.SearchParameters.OrderBy!, out var enumOrderBySelectorValue))
        {
            return searchModel.SearchParameters.OrderAsc!.Value
                ? queryable.OrderBy(enumOrderBySelectorValue)
                : queryable.OrderByDescending(enumOrderBySelectorValue);
        }

        var orderBySelector = selectorDict[searchModel.SearchParameters.OrderBy!];
        var isBoolean = (orderBySelector.Body as UnaryExpression)!.Operand.Type == typeof(bool);
        return searchModel.SearchParameters.OrderAsc!.Value
            ? isBoolean ? queryable.OrderByDescending(orderBySelector) : queryable.OrderBy(orderBySelector)
            : isBoolean ? queryable.OrderBy(orderBySelector) : queryable.OrderByDescending(orderBySelector);
    }

    private IOrderedQueryable<TModel> GetThenOrderedQuery(IOrderedQueryable<TModel> orderedQuery,
        IReadOnlyDictionary<string, Expression<Func<TModel, object>>> selectorDict,
        IReadOnlyDictionary<string, Expression<Func<TModel, int>>> enumSelectorDict,
        TSearchModel searchModel)
    {
        if (enumSelectorDict.TryGetValue(searchModel.SearchParameters.ThenBy!, out var enumThenBySelectorValue))
        {
            return searchModel.SearchParameters.ThenAsc == true
                ? orderedQuery.ThenBy(enumThenBySelectorValue)
                : orderedQuery.ThenByDescending(enumThenBySelectorValue);
        }

        if (!selectorDict.TryGetValue(searchModel.SearchParameters.ThenBy!, out var thenBySelectorValue))
        {
            return orderedQuery;
        }
        var isBoolean = (thenBySelectorValue.Body as UnaryExpression)!.Operand.Type == typeof(bool);
        return searchModel.SearchParameters.ThenAsc == true
            ? isBoolean ? orderedQuery.ThenByDescending(thenBySelectorValue) : orderedQuery.ThenBy(thenBySelectorValue)
            : isBoolean ? orderedQuery.ThenBy(thenBySelectorValue) : orderedQuery.ThenByDescending(thenBySelectorValue);

    }

    protected abstract Expression<Func<TModel, object>> DefaultOrderExpression { get; }
}
