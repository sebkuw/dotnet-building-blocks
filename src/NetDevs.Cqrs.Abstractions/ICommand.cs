namespace NetDevs.Cqrs.Abstractions;

/// <summary>
/// Marks a command that does not return a response value.
/// </summary>
public interface ICommand;

/// <summary>
/// Marks a command that returns a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The command response type.</typeparam>
public interface ICommand<TResponse>;
