using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace sebkuw.ImportProcessor;

/// <summary>Declaratively maps CSV columns to newly created values.</summary>
public sealed class ImportMap<T>
{
    private readonly List<IColumnBinding<T>> bindings = [];
    private readonly List<IImportRowValidator<T>> validators = [];

    /// <summary>Creates a map that uses <paramref name="factory"/> for every row.</summary>
    public ImportMap(Func<T> factory)
    {
        Factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    internal Func<T> Factory { get; }

    internal IReadOnlyList<IColumnBinding<T>> Bindings => bindings;

    internal IReadOnlyList<IImportRowValidator<T>> Validators => validators;

    internal Func<T, string?>? IdempotencyKeySelector { get; private set; }

    /// <summary>Maps a column using an assignment delegate and optional converter.</summary>
    public ImportMap<T> Map<TValue>(
        string column,
        Action<T, TValue> assign,
        Func<string, CultureInfo, TValue>? convert = null,
        bool required = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(column);
        ArgumentNullException.ThrowIfNull(assign);
        EnsureUnique(column);
        bindings.Add(new ColumnBinding<T, TValue>(column, assign, convert ?? ValueConverter.Convert<TValue>, required));
        return this;
    }

    /// <summary>Maps a column to a writable property and optional converter.</summary>
    public ImportMap<T> Map<TValue>(
        string column,
        Expression<Func<T, TValue>> property,
        Func<string, CultureInfo, TValue>? convert = null,
        bool required = true)
    {
        ArgumentNullException.ThrowIfNull(property);
        if (property.Body is not MemberExpression { Member: PropertyInfo propertyInfo } memberExpression ||
            memberExpression.Expression != property.Parameters[0] ||
            propertyInfo.SetMethod is null ||
            propertyInfo.SetMethod.IsStatic)
            throw new ArgumentException("Expression must select a writable instance property.", nameof(property));

        var target = Expression.Parameter(typeof(T), "target");
        var value = Expression.Parameter(typeof(TValue), "value");
        var setter = Expression.Lambda<Action<T, TValue>>(
            Expression.Assign(Expression.Property(target, propertyInfo), value),
            target,
            value).Compile();
        return Map(column, setter, convert, required);
    }

    /// <summary>Adds an asynchronous row validator.</summary>
    public ImportMap<T> ValidateWith(IImportRowValidator<T> validator)
    {
        ArgumentNullException.ThrowIfNull(validator);
        validators.Add(validator);
        return this;
    }

    /// <summary>Selects the application-defined idempotency key for a mapped value.</summary>
    public ImportMap<T> UseIdempotencyKey(Func<T, string?> selector)
    {
        IdempotencyKeySelector = selector ?? throw new ArgumentNullException(nameof(selector));
        return this;
    }

    private void EnsureUnique(string column)
    {
        if (bindings.Any(binding => string.Equals(binding.Column, column, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"Column '{column}' is already mapped.", nameof(column));
    }

    internal interface IColumnBinding<in TTarget>
    {
        string Column { get; }

        bool Required { get; }

        void Assign(TTarget target, string value, CultureInfo culture);
    }

    private sealed record ColumnBinding<TTarget, TValue>(
        string Column,
        Action<TTarget, TValue> Setter,
        Func<string, CultureInfo, TValue> Converter,
        bool Required) : IColumnBinding<TTarget>
    {
        public void Assign(TTarget target, string value, CultureInfo culture) => Setter(target, Converter(value, culture));
    }

    private static class ValueConverter
    {
        public static TValue Convert<TValue>(string value, CultureInfo culture)
        {
            var targetType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);
            if (string.IsNullOrEmpty(value) && Nullable.GetUnderlyingType(typeof(TValue)) is not null)
                return default!;

            if (targetType == typeof(string))
                return (TValue)(object)value;

            if (targetType.IsEnum)
                return (TValue)Enum.Parse(targetType, value, ignoreCase: true);

            var converter = TypeDescriptor.GetConverter(targetType);
            if (!converter.CanConvertFrom(typeof(string)))
                throw new NotSupportedException($"No string converter is available for '{targetType.Name}'.");

            return (TValue)converter.ConvertFrom(null, culture, value)!;
        }
    }
}
