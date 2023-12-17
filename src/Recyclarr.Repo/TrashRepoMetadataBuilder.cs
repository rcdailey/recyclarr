using System.IO.Abstractions;
using System.Text.Json;
using Recyclarr.Json;

namespace Recyclarr.Repo;

public class TrashRepoMetadataBuilder(ITrashGuidesRepo repo) : IRepoMetadataBuilder
{
    private RepoMetadata? _metadata;
    private readonly IDirectoryInfo _repoPath = repo.Path;

    public IFileInfo MetadataPath => _repoPath.File("metadata.json");
    public IDirectoryInfo DocsDirectory => _repoPath.SubDirectory("docs");

    private static RepoMetadata Deserialize(IFileInfo jsonFile)
    {
        using var stream = jsonFile.OpenRead();

        var obj = JsonSerializer.Deserialize<RepoMetadata>(stream, GlobalJsonSerializerSettings.Guide);
        if (obj is null)
        {
            throw new InvalidDataException($"Unable to deserialize {jsonFile}");
        }

        return obj;
    }

    public IReadOnlyList<IDirectoryInfo> ToDirectoryInfoList(IEnumerable<string> listOfDirectories)
    {
        return listOfDirectories.Select(x => _repoPath.SubDirectory(x)).ToList();
    }

    public RepoMetadata GetMetadata()
    {
        return _metadata ??= Deserialize(MetadataPath);
    }
}
