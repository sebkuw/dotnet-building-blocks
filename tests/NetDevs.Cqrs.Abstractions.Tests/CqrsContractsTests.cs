using Xunit;

namespace NetDevs.Cqrs.Abstractions.Tests;

public sealed class CqrsContractsTests
{
    [Fact]
    public async Task Command_handler_contract_forwards_command_and_cancellation_token()
    {
        using var cancellation = new CancellationTokenSource();
        var command = new TestCommand();
        var handler = new TestCommandHandler();

        await handler.Handle(command, cancellation.Token);

        Assert.Same(command, handler.Command);
        Assert.Equal(cancellation.Token, handler.CancellationToken);
    }

    [Fact]
    public async Task Command_with_response_handler_returns_typed_response()
    {
        ICommandHandler<TestCommandWithResponse, Guid> handler = new TestCommandWithResponseHandler();

        Guid response = await handler.Handle(new TestCommandWithResponse(), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, response);
    }

    [Fact]
    public async Task Query_handler_returns_typed_response()
    {
        IQueryHandler<TestQuery, string> handler = new TestQueryHandler();

        string response = await handler.Handle(new TestQuery(), CancellationToken.None);

        Assert.Equal("result", response);
    }

    private sealed record TestCommand : ICommand;

    private sealed record TestCommandWithResponse : ICommand<Guid>;

    private sealed record TestQuery : IQuery<string>;

    private sealed class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public TestCommand? Command { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task Handle(TestCommand command, CancellationToken cancellationToken)
        {
            Command = command;
            CancellationToken = cancellationToken;
            return Task.CompletedTask;
        }
    }

    private sealed class TestCommandWithResponseHandler : ICommandHandler<TestCommandWithResponse, Guid>
    {
        public Task<Guid> Handle(TestCommandWithResponse command, CancellationToken cancellationToken)
        {
            return Task.FromResult(Guid.NewGuid());
        }
    }

    private sealed class TestQueryHandler : IQueryHandler<TestQuery, string>
    {
        public Task<string> Handle(TestQuery query, CancellationToken cancellationToken)
        {
            return Task.FromResult("result");
        }
    }
}
