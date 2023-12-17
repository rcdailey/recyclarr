using System.IO.Abstractions;
using Recyclarr.Repo;
using Recyclarr.TestLibrary;

namespace Recyclarr.Cli.IntegrationTests;

public class StubTrashRepoMetadataBuilder : IRepoMetadataBuilder
{
    private readonly IRepoMetadataBuilder _metadata;

    private const string MetadataJson =
        """
        {
          "json_paths": {
            "radarr": {
              "custom_formats": ["docs/json/radarr/cf"],
              "qualities": ["docs/json/radarr/quality-size"],
              "naming": ["docs/json/radarr/naming"]
            },
            "sonarr": {
              "release_profiles": ["docs/json/sonarr/rp"],
              "custom_formats": ["docs/json/sonarr/cf"],
              "qualities": ["docs/json/sonarr/quality-size"],
              "naming": ["docs/json/sonarr/naming"]
            }
          },
          "recyclarr": {
            "templates": "docs/recyclarr-configs"
          }
        }
        """;

    public StubTrashRepoMetadataBuilder(MockFileSystem fs, IRepoMetadataBuilder metadata)
    {
        _metadata = metadata;
        fs.AddFile(MetadataPath, new MockFileData(MetadataJson));
        fs.AddSameFileFromEmbeddedResource(
            DocsDirectory.SubDirectory("Radarr").File("Radarr-collection-of-custom-formats.md"),
            typeof(StubTrashRepoMetadataBuilder));
        fs.AddSameFileFromEmbeddedResource(
            DocsDirectory.SubDirectory("Sonarr").File("sonarr-collection-of-custom-formats.md"),
            typeof(StubTrashRepoMetadataBuilder));
    }

    public IDirectoryInfo DocsDirectory => _metadata.DocsDirectory;
    public IFileInfo MetadataPath => _metadata.MetadataPath;

    public RepoMetadata GetMetadata() => _metadata.GetMetadata();

    public IReadOnlyList<IDirectoryInfo> ToDirectoryInfoList(IEnumerable<string> listOfDirectories)
        => _metadata.ToDirectoryInfoList(listOfDirectories);
}
