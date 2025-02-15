using System.IO.Abstractions;
using Recyclarr.Platform;
using IEnvironment = Recyclarr.Platform.IEnvironment;

namespace Recyclarr.Core.Tests.UnitTests.Platform;

[TestFixture]
public class DefaultAppDataSetupTest
{
    [Test, AutoMockData]
    public void Initialize_using_default_path(
        [Frozen(Matching.ImplementedInterfaces)] MockFileSystem fs,
        [Frozen] IEnvironment env,
        DefaultAppDataSetup sut
    )
    {
        env.GetEnvironmentVariable(default!).ReturnsForAnyArgs((string?)null);

        var basePath = fs.CurrentDirectory().SubDirectory("base").SubDirectory("path");

        env.GetFolderPath(default, default).ReturnsForAnyArgs(basePath.FullName);
        sut.SetAppDataDirectoryOverride("");

        var paths = sut.CreateAppPaths();

        paths.AppDataDirectory.FullName.Should().Be(basePath.SubDirectory("recyclarr").FullName);
    }

    [Test, AutoMockData]
    public void Initialize_using_path_override(
        [Frozen(Matching.ImplementedInterfaces)] MockFileSystem fs,
        DefaultAppDataSetup sut
    )
    {
        var overridePath = fs.CurrentDirectory().SubDirectory("override").SubDirectory("path");

        sut.SetAppDataDirectoryOverride(overridePath.FullName);
        var paths = sut.CreateAppPaths();

        paths.AppDataDirectory.FullName.Should().Be(overridePath.FullName);
    }

    [Test, AutoMockData]
    public void Creation_uses_correct_behavior(
        [Frozen(Matching.ImplementedInterfaces)] MockFileSystem fs,
        [Frozen] IEnvironment env,
        DefaultAppDataSetup sut
    )
    {
        var appDataPath = fs.CurrentDirectory().SubDirectory("override").SubDirectory("path");

        env.GetEnvironmentVariable(default!).ReturnsForAnyArgs((string?)null);
        env.GetFolderPath(default).ReturnsForAnyArgs(appDataPath.FullName);
        sut.SetAppDataDirectoryOverride("");

        sut.CreateAppPaths();

        env.Received()
            .GetFolderPath(
                Environment.SpecialFolder.ApplicationData,
                Environment.SpecialFolderOption.Create
            );
        fs.AllDirectories.Should().NotContain(appDataPath.FullName);
    }

    [Test, AutoMockData]
    public void Use_environment_variable_if_override_not_specified(
        [Frozen(Matching.ImplementedInterfaces)] MockFileSystem fs,
        [Frozen] IEnvironment env,
        DefaultAppDataSetup sut
    )
    {
        var expectedPath = fs.CurrentDirectory()
            .SubDirectory("env")
            .SubDirectory("var")
            .SubDirectory("path")
            .FullName;

        env.GetEnvironmentVariable(default!).ReturnsForAnyArgs(expectedPath);
        sut.SetAppDataDirectoryOverride("");

        sut.CreateAppPaths();

        env.Received().GetEnvironmentVariable("RECYCLARR_APP_DATA");
        fs.AllDirectories.Should().Contain(expectedPath);
    }

    [Test, AutoMockData]
    public void Explicit_override_takes_precedence_over_environment_variable(
        [Frozen(Matching.ImplementedInterfaces)] MockFileSystem fs,
        [Frozen] IEnvironment env,
        DefaultAppDataSetup sut
    )
    {
        var expectedPath = fs.CurrentDirectory()
            .SubDirectory("env")
            .SubDirectory("var")
            .SubDirectory("path")
            .FullName;

        sut.SetAppDataDirectoryOverride(expectedPath);

        sut.CreateAppPaths();

        env.DidNotReceiveWithAnyArgs().GetEnvironmentVariable(default!);
        fs.AllDirectories.Should().Contain(expectedPath);
    }
}
