using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

using NetDevs.QueryableProcessor.Enums;
using NetDevs.QueryableProcessor.Models;

namespace NetDevs.QueryableProcessor.Extensions;

/// <summary>
/// Provides dynamic filtering operations for queryable sources.
/// </summary>
public static class QueryableFilter
{
    /// <summary>
    /// Applies multiple dynamic filters to an IQueryable collection.
    /// Each filter consists of a property path, a comparison operator, and a value.
    /// </summary>
    /// <typeparam name="T">The type of elements in the IQueryable collection.</typeparam>
    /// <param name="source">The IQueryable source to filter.</param>
    /// <param name="filters">
    /// A collection of filter conditions to apply dynamically.
    /// If null or empty, no filtering will be performed.
    /// </param>
    /// <returns>
    /// An IQueryable collection with all specified filters applied.
    /// If no filters are provided, returns the original source unmodified.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when the source is null.</exception>
    public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> source, IEnumerable<FilterCondition>? filters)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (filters == null || !filters.Any())
            return source;

        foreach (FilterCondition filter in filters)
            source = source.FilterProp(filter.PropertyPath, filter.Operation, filter.Value);

        return source;
    }

    /// <summary>
    /// Applies a dynamic filter to an IQueryable collection based on the provided property path, operation, and value.
    /// </summary>
    /// <typeparam name="T">The type of elements in the IQueryable collection.</typeparam>
    /// <param name="source">The IQueryable source to filter.</param>
    /// <param name="propertyPath">The property name or path (e.g., "User.Address.City").</param>
    /// <param name="operation">The filter operation, such as equality, comparison, collection, or string matching.</param>
    /// <param name="value">The value to filter by. For "in" and "notIn", provide a list.</param>
    /// <returns>A filtered IQueryable collection.</returns>
    /// <exception cref="ArgumentException">Thrown when an invalid operation is provided.</exception>
    private static IQueryable<T> FilterProp<T>(this IQueryable<T> source, string propertyPath, FilterOperation operation, object value)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyPath);

        ParameterExpression param = Expression.Parameter(typeof(T), "x");
        Expression property = param;

        // Navigate through nested properties (for example, "User.Name").
        foreach (string part in propertyPath.Split('.'))
        {
            PropertyInfo? propertyInfo = property.Type.GetProperty(
                part,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (propertyInfo is null)
                throw new ArgumentException($"Property path '{propertyPath}' does not exist.", nameof(propertyPath));

            property = Expression.Property(property, propertyInfo);
        }

        Expression comparison;

        ConstantExpression CreateConstant()
        {
            return Expression.Constant(ConvertValueForExpression(value, property.Type), property.Type);
        }

        switch (operation)
        {
            case FilterOperation.Equal:
                comparison = Expression.Equal(property, CreateConstant());
                break;
            case FilterOperation.NotEqual:
                comparison = Expression.NotEqual(property, CreateConstant());
                break;
            case FilterOperation.GreaterThan:
                comparison = Expression.GreaterThan(property, CreateConstant());
                break;
            case FilterOperation.LessThan:
                comparison = Expression.LessThan(property, CreateConstant());
                break;
            case FilterOperation.GreaterThanOrEqual:
                comparison = Expression.GreaterThanOrEqual(property, CreateConstant());
                break;
            case FilterOperation.LessThanOrEqual:
                comparison = Expression.LessThanOrEqual(property, CreateConstant());
                break;
            case FilterOperation.Contains:
                if (property.Type == typeof(string))
                    comparison = Expression.Call(property, typeof(string).GetMethod("Contains", [typeof(string)])!, CreateConstant());
                else
                    throw new ArgumentException("Contains can only be used with strings.");
                break;
            case FilterOperation.StartsWith:
                if (property.Type == typeof(string))
                    comparison = Expression.Call(property, typeof(string).GetMethod("StartsWith", [typeof(string)])!, CreateConstant());
                else
                    throw new ArgumentException("StartsWith can only be used with strings.");
                break;
            case FilterOperation.EndsWith:
                if (property.Type == typeof(string))
                    comparison = Expression.Call(property, typeof(string).GetMethod("EndsWith", [typeof(string)])!, CreateConstant());
                else
                    throw new ArgumentException("EndsWith can only be used with strings.");
                break;
            case FilterOperation.In:
                comparison = BuildContainsExpression(value, property);
                break;
            case FilterOperation.NotIn:
                comparison = Expression.Not(BuildContainsExpression(value, property));
                break;
            default:
                throw new ArgumentException($"Unsupported operation: {operation}");
        }

        Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(comparison, param);

        return source.Where(lambda);
    }

    /// <summary>
    /// Builds a strongly typed collection membership expression.
    /// </summary>
    /// <param name="value">The collection supplied by the filter.</param>
    /// <param name="property">The property checked for membership.</param>
    /// <returns>A call to the typed collection's Contains method.</returns>
    private static MethodCallExpression BuildContainsExpression(object value, Expression property)
    {
        IEnumerable<object?> rawValues = value switch
        {
            JsonElement { ValueKind: JsonValueKind.Array } jsonArray => jsonArray.EnumerateArray().Cast<object?>(),
            System.Collections.IEnumerable enumerable when value is not string => enumerable.Cast<object?>(),
            _ => throw new ArgumentException("In and NotIn operations require a collection value.", nameof(value))
        };

        Type listType = typeof(List<>).MakeGenericType(property.Type);
        System.Collections.IList values = (System.Collections.IList)Activator.CreateInstance(listType)!;

        foreach (object? rawValue in rawValues)
            values.Add(ConvertValueForExpression(rawValue, property.Type));

        MethodInfo containsMethod = listType.GetMethod("Contains", [property.Type])!;

        return Expression.Call(Expression.Constant(values, listType), containsMethod, property);
    }

    private static object? ConvertValueForExpression(object? value, Type targetType)
    {
        Type effectiveTargetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (value is null)
            return targetType.IsValueType && Nullable.GetUnderlyingType(targetType) is null
                ? Activator.CreateInstance(targetType)
                : null;

        if (value is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Null)
                return null;

            if (effectiveTargetType.IsEnum)
            {
                return jsonElement.ValueKind == JsonValueKind.String
                    ? Enum.Parse(effectiveTargetType, jsonElement.GetString()!, ignoreCase: true)
                    : Enum.ToObject(effectiveTargetType, jsonElement.GetInt32());
            }

            if (effectiveTargetType == typeof(int))
                return jsonElement.GetInt32();
            if (effectiveTargetType == typeof(long))
                return jsonElement.GetInt64();
            if (effectiveTargetType == typeof(string))
                return jsonElement.GetString();
            if (effectiveTargetType == typeof(bool))
                return jsonElement.GetBoolean();
            if (effectiveTargetType == typeof(Guid))
                return jsonElement.GetGuid();
            if (effectiveTargetType == typeof(DateTime))
                return jsonElement.GetDateTime();
        }

        if (effectiveTargetType.IsEnum)
            return value is string enumName
                ? Enum.Parse(effectiveTargetType, enumName, ignoreCase: true)
                : Enum.ToObject(effectiveTargetType, value);
        else if (effectiveTargetType == typeof(string))
            return value?.ToString();
        else if (effectiveTargetType == typeof(Guid) && value is string stringValue)
        {
            if (Guid.TryParse(stringValue, out Guid guid))
                return guid;
            throw new FormatException($"'{stringValue}' is not a valid GUID value.");
        }
        else
            return Convert.ChangeType(value, effectiveTargetType, CultureInfo.InvariantCulture);
    }
}
