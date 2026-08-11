using Microsoft.Extensions.DependencyInjection;

using NetDevs.Cqrs.Abstractions;

using Xunit;

namespace NetDevs.Cqrs.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCqrs_throws_when_no_assemblies_are_configured()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddCqrs(_ => { }));

        Assert.Equal(
            "No assemblies were configured for CQRS registration.",
            exception.Message);
    }

    [Fact]
    public void AddCqrs_registers_command_and_query_handlers_from_configured_assembly()
    {
        var services = new ServiceCollection();

        services.AddCqrs(options =>
            options.RegisterServicesFromAssemblyContaining<ServiceCollectionExtensionsTests>());

        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();

        Assert.IsType<TestCommandHandler>(
            scope.ServiceProvider.GetRequiredService<ICommandHandler<TestCommand>>());
        Assert.IsType<TestCommandWithResponseHandler>(
            scope.ServiceProvider.GetRequiredService<ICommandHandler<TestCommandWithResponse, Guid>>());
        Assert.IsType<TestQueryHandler>(
            scope.ServiceProvider.GetRequiredService<IQueryHandler<TestQuery, string>>());
    }

    [Fact]
    public void AddCqrs_registers_handlers_as_scoped_services()
    {
        var services = new ServiceCollection();

        services.AddCqrsFromAssemblyContaining<ServiceCollectionExtensionsTests>();

        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();

        var firstResolution = firstScope.ServiceProvider
            .GetRequiredService<IQueryHandler<TestQuery, string>>();
        var secondResolution = firstScope.ServiceProvider
            .GetRequiredService<IQueryHandler<TestQuery, string>>();
        var thirdResolution = secondScope.ServiceProvider
            .GetRequiredService<IQueryHandler<TestQuery, string>>();

        Assert.Same(firstResolution, secondResolution);
        Assert.NotSame(firstResolution, thirdResolution);
    }

    private sealed record TestCommand : ICommand;

    private sealed class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public Task Handle(TestCommand command, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed record TestCommandWithResponse : ICommand<Guid>;

    private sealed class TestCommandWithResponseHandler
        : ICommandHandler<TestCommandWithResponse, Guid>
    {
        public Task<Guid> Handle(
            TestCommandWithResponse command,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Guid.NewGuid());
        }
    }

    private sealed record TestQuery : IQuery<string>;

    private sealed class TestQueryHandler : IQueryHandler<TestQuery, string>
    {
        public Task<string> Handle(TestQuery query, CancellationToken cancellationToken)
        {
            return Task.FromResult("test");
        }
    }
}
