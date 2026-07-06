using NetDevs.QueryableProcessor.Constants;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NetDevs.QueryableProcessor.Extensions;

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
    /// Prefixes:
    /// - "asc_" for ascending order.
    /// - "desc_" for descending order.
    /// Example: "desc_CreatedAt" sorts by `CreatedAt` in descending order.
    /// If null or invalid return not changed source.
    /// </param>
    /// <returns>A sorted IQueryable collection.</returns>
    public static IQueryable<T> SortBy<T>(this IQueryable<T> source, string propertyPath)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (string.IsNullOrWhiteSpace(propertyPath))
            return source;

        string method = SettingsConstants.OrderByMethod;

        if (propertyPath.StartsWith(SettingsConstants.AscendingPrefix))
            propertyPath = propertyPath[4..];
        else if (propertyPath.StartsWith(SettingsConstants.DescendingPrefix))
        {
            propertyPath = propertyPath[5..];
            method = SettingsConstants.OrderByDescendingMethod;
        }
        else
            return source;

        return ApplySorting(source, propertyPath, method);
    }

    private static IQueryable<T> ApplySorting<T>(IQueryable<T> source, string propertyPath, string method)
    {
        ParameterExpression param = Expression.Parameter(typeof(T), "x");
        Expression property = param;

        // Navigate through nested properties (e.g., "User.Name")
        foreach (string part in propertyPath.Split('.'))
        {
            PropertyInfo? propertyInfo = property.Type.GetProperty(part);
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
