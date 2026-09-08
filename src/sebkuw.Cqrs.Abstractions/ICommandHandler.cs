namespace sebkuw.Cqrs.Abstractions;

/// <summary>
/// Handles a command that does not return a response value.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    /// <summary>Handles the command.</summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    Task Handle(TCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Handles a command that returns a typed response.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    /// <summary>Handles the command and returns its response.</summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>The command response.</returns>
    Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken);
}
