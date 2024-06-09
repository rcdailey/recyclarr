using System.IO.Abstractions;
using Autofac;
using Recyclarr.Cli.Console.Commands;
using Recyclarr.Common.Extensions;
using Recyclarr.Platform;
using Recyclarr.TestLibrary.Autofac;
using Recyclarr.TestLibrary.AutoFixture;
using Spectre.Console.Cli;

namespace Recyclarr.Cli.IntegrationTests.Tests;

[TestFixture]
public class PlatformIntegrationTest : IntegrationTestFixture2
{
    [Test, AutoMockData]
    public async Task Sync_successful(CommandContext ctx)
    {
        // The "old" app data directory has to be in place before we create SyncCommand, since we want the
        // automatic/transparent move of that directory to happen before the migration system checks it.
        var env = Resolve<IEnvironment>();

        var oldPath = new AppPaths(Fs.DirectoryInfo
            .New(env.GetFolderPath(Environment.SpecialFolder.UserProfile))
            .SubDirectory(".config", AppPaths.DefaultAppDataDirectoryName));

        Fs.AddFile(oldPath.ConfigsDirectory.File("test.yml"), new MockFileData(
            """
            radarr:
              test_instance:
                base_url: http://localhost:1000
                api_key: key
            """));

        // Fs.AddEmptyFile(oldPath.File("test.yml"));

        // The runtime info mocking has to happen before the instantiation of SyncCommand.
        var runtimeInfo = Resolve<IRuntimeInformation>();
        runtimeInfo.IsPlatformOsx().Returns(true);

        var cmd = Resolve<SyncCommand>();
        var settings = new SyncCommand.CliSettings();
        var newPath = Resolve<IAppPaths>();

        var returnCode = await cmd.ExecuteAsync(ctx, settings);

        returnCode.Should().Be(0);
        Fs.AllFiles.Should().Contain(newPath.ConfigsDirectory.SubDirectory("configs").File("test.yml").FullName);
    }
}
