using System.Linq.Expressions;
using System.Reflection;

using sebkuw.QueryableProcessor.Constants;

namespace sebkuw.QueryableProcessor.Extensions;

/// <summary>
/// Provides dynamic sorting for IQueryable collections.
/// </summary>
public static class QueryableSort
{
    /// <summary>
    /// Applies dynamic sorting to an IQueryable collection based on the specified property path.
    /// If propertyPath is null, empty, or invalid, sorting defaults to "desc_Id".
    /// </summary>
    /// <typeparam name="T">The type of elements in the IQueryable collection.</typeparam>
    /// <param name="source">The IQueryable source to sort.</param>
    /// <param name="propertyPath">
    /// The property name or path to sort by.
    /// Supported formats:
    /// - "asc_Property" or "desc_Property".
    /// - "Property asc" or "Property desc", as emitted by @netdevs/shared-ui-list.
    /// Property lookup is case-insensitive and supports nested paths.
    /// If null or invalid return not changed source.
    /// </param>
    /// <returns>A sorted IQueryable collection.</returns>
    public static IQueryable<T> SortBy<T>(this IQueryable<T> source, string propertyPath)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (string.IsNullOrWhiteSpace(propertyPath))
            return source;

        if (!TryParseSort(propertyPath, out string normalizedPath, out string method))
            return source;

        return ApplySorting(source, normalizedPath, method);
    }

    private static bool TryParseSort(string sort, out string propertyPath, out string method)
    {
        string value = sort.Trim();
        method = SettingsConstants.OrderByMethod;

        if (value.StartsWith(SettingsConstants.AscendingPrefix, StringComparison.OrdinalIgnoreCase))
        {
            propertyPath = value[SettingsConstants.AscendingPrefix.Length..];
        }
        else if (value.StartsWith(SettingsConstants.DescendingPrefix, StringComparison.OrdinalIgnoreCase))
        {
            propertyPath = value[SettingsConstants.DescendingPrefix.Length..];
            method = SettingsConstants.OrderByDescendingMethod;
        }
        else if (value.EndsWith(" asc", StringComparison.OrdinalIgnoreCase))
        {
            propertyPath = value[..^4].TrimEnd();
        }
        else if (value.EndsWith(" desc", StringComparison.OrdinalIgnoreCase))
        {
            propertyPath = value[..^5].TrimEnd();
            method = SettingsConstants.OrderByDescendingMethod;
        }
        else
        {
            propertyPath = string.Empty;
            return false;
        }

        return !string.IsNullOrWhiteSpace(propertyPath);
    }

    private static IQueryable<T> ApplySorting<T>(IQueryable<T> source, string propertyPath, string method)
    {
        ParameterExpression param = Expression.Parameter(typeof(T), "x");
        Expression property = param;

        // Navigate through nested properties (e.g., "User.Name")
        foreach (string part in propertyPath.Split('.'))
        {
            PropertyInfo? propertyInfo = property.Type.GetProperty(
                part,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (propertyInfo is null)
                return source; // Property doesn't exist, return unchanged

            property = Expression.Property(property, propertyInfo);
        }

        LambdaExpression lambda = Expression.Lambda(property, param);

        object? result = typeof(Queryable).GetMethods()
            .First(m => m.Name == method && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), property.Type)
            .Invoke(null, [source, lambda]);

        return (IQueryable<T>?)result ?? throw new InvalidOperationException($"Failed to apply {method} sorting.");
    }
}
