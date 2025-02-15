using System.IO.Abstractions;
using Recyclarr.Config.Parsing.PostProcessing.ConfigMerging;
using Recyclarr.Platform;

namespace Recyclarr.Core.Tests.UnitTests.Config.Parsing.PostProcessing.ConfigMerging;

public class ConfigIncludeProcessorTest
{
    [Test, AutoMockData]
    public void Throw_when_null_include_path(ConfigIncludeProcessor sut)
    {
        var includeDirective = new ConfigYamlInclude { Config = null };

        var act = () => sut.GetPathToConfig(includeDirective, default);

        act.Should().Throw<YamlIncludeException>();
    }

    [Test, AutoMockData]
    public void Get_relative_config_include_path(
        [Frozen(Matching.ImplementedInterfaces)] MockFileSystem fs,
        [Frozen] IAppPaths paths,
        ConfigIncludeProcessor sut
    )
    {
        fs.AddEmptyFile(paths.IncludesDirectory.File("foo/bar/config.yml"));

        var includeDirective = new ConfigYamlInclude { Config = "foo/bar/config.yml" };

        var path = sut.GetPathToConfig(includeDirective, default);

        path.FullName.Should().Be(paths.IncludesDirectory.File("foo/bar/config.yml").FullName);
    }

    [Test, AutoMockData]
    public void Get_absolute_config_include_path(
        [Frozen(Matching.ImplementedInterfaces)] MockFileSystem fs,
        ConfigIncludeProcessor sut
    )
    {
        var absolutePath = fs.CurrentDirectory().File("foo/bar/config.yml");
        fs.AddEmptyFile(absolutePath);

        var includeDirective = new ConfigYamlInclude { Config = absolutePath.FullName };

        var path = sut.GetPathToConfig(includeDirective, default);

        path.FullName.Should().Be(absolutePath.FullName);
    }

    [Test, AutoMockData]
    public void Throw_when_relative_config_include_path_does_not_exist(
        // Freeze the mock FS even though we don't use it so that the "Exists" check works right.
        [Frozen(Matching.ImplementedInterfaces)]
            MockFileSystem fs,
        ConfigIncludeProcessor sut
    )
    {
        var includeDirective = new ConfigYamlInclude { Config = "foo/bar/config.yml" };

        var act = () => sut.GetPathToConfig(includeDirective, default);

        act.Should().Throw<YamlIncludeException>();
    }

    [Test, AutoMockData]
    public void Throw_when_absolute_config_include_path_does_not_exist(
        [Frozen(Matching.ImplementedInterfaces)] MockFileSystem fs,
        ConfigIncludeProcessor sut
    )
    {
        var absolutePath = fs.CurrentDirectory().File("foo/bar/config.yml");

        var includeDirective = new ConfigYamlInclude { Config = absolutePath.FullName };

        var act = () => sut.GetPathToConfig(includeDirective, default);

        act.Should().Throw<YamlIncludeException>();
    }
}
