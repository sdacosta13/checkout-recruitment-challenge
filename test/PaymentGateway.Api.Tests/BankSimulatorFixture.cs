using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace PaymentGateway.Api.Tests;

[CollectionDefinition(Name)]
public class BankSimulatorCollection : ICollectionFixture<BankSimulatorFixture>
{
    public const string Name = "BankSimulator";
}

public class BankSimulatorFixture : IAsyncLifetime
{
    private IContainer _container = null!;

    public Uri BaseAddress { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var impostersPath = Path.Combine(AppContext.BaseDirectory, "imposters");

        _container = new ContainerBuilder("bbyars/mountebank:2.8.1")
            .WithPortBinding(8080, true)
            .WithBindMount(impostersPath, "/imposters")
            .WithCommand("--configfile", "/imposters/bank_simulator.ejs", "--allowInjection")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("now taking orders"))
            .Build();

        await _container.StartAsync();

        BaseAddress = new Uri($"http://localhost:{_container.GetMappedPublicPort(8080)}");
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();
}
