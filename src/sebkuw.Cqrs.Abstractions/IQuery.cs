namespace sebkuw.Cqrs.Abstractions;

/// <summary>
/// Marks a query that returns a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The query response type.</typeparam>
public interface IQuery<TResponse>;
