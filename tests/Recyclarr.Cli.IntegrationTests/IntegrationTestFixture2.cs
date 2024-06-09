using System.IO.Abstractions;
using Autofac;
using Autofac.Features.ResolveAnything;
using Recyclarr.Compatibility;
using Recyclarr.Platform;
using Recyclarr.Repo;
using Recyclarr.TestLibrary;
using Recyclarr.TestLibrary.Autofac;
using Recyclarr.VersionControl;
using Spectre.Console;
using Spectre.Console.Testing;

namespace Recyclarr.Cli.IntegrationTests;

public abstract class IntegrationTestFixture2 : IDisposable
{
    private readonly Lazy<ILifetimeScope> _container;

    protected ILifetimeScope Container => _container.Value;
    protected MockFileSystem Fs { get; }
    protected TestConsole Console { get; } = new();
    protected TestableLogger Logger { get; } = new();

    protected IntegrationTestFixture2()
    {
        Fs = new MockFileSystem(new MockFileSystemOptions
        {
            CreateDefaultTempDir = false
        });

        // Use Lazy because we shouldn't invoke virtual methods at construction time
        _container = new Lazy<ILifetimeScope>(() =>
        {
            var builder = new ContainerBuilder();
            RegisterTypes(builder);
            RegisterStubsAndMocks(builder);
            builder.RegisterSource<AnyConcreteTypeNotAlreadyRegisteredSource>();
            return builder.Build();
        });
    }

    /// <summary>
    /// Register "real" types (usually Module-derived classes from other projects). This call happens
    /// before
    /// RegisterStubsAndMocks().
    /// </summary>
    protected virtual void RegisterTypes(ContainerBuilder builder)
    {
        CompositionRoot.Setup(builder);
    }

    /// <summary>
    /// Override registrations made in the RegisterTypes() method. This method is called after
    /// RegisterTypes().
    /// </summary>
    protected virtual void RegisterStubsAndMocks(ContainerBuilder builder)
    {
        builder.RegisterType<StubEnvironment>().As<IEnvironment>();

        builder.RegisterInstance(Fs).As<IFileSystem>().AsSelf();
        builder.RegisterInstance(Console).As<IAnsiConsole>();
        builder.RegisterInstance(Logger).As<ILogger>();

        // Platform related mocks
        builder.RegisterMockFor<IRuntimeInformation>();

        builder.RegisterMockFor<IGitRepository>();
        builder.RegisterMockFor<IGitRepositoryFactory>();
        builder.RegisterMockFor<IServiceInformation>(m =>
        {
            // By default, choose some extremely high number so that all the newest features are enabled.
            m.GetVersion().ReturnsForAnyArgs(_ => new Version("99.0.0.0"));
        });

        builder.RegisterDecorator<StubTrashRepoMetadataBuilder, IRepoMetadataBuilder>();
    }

    [TearDown]
    public void DumpLogsToConsole()
    {
        foreach (var line in Console.Lines)
        {
            System.Console.WriteLine(line);
        }
    }

    protected T Resolve<T>() where T : notnull
    {
        return Container.Resolve<T>();
    }

    // ReSharper disable once VirtualMemberNeverOverridden.Global
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing || !_container.IsValueCreated)
        {
            return;
        }

        _container.Value.Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
