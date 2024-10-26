using System.IO.Abstractions;
using Recyclarr.Json.Loading;
using Recyclarr.Repo;
using Recyclarr.TrashGuide.CustomFormat;
using Recyclarr.TrashGuide.MediaNaming;

namespace Recyclarr.TrashGuide.CustomFormatGroups;

public class CustomFormatGroupsGuideService(
    IRepoMetadataBuilder metadataBuilder,
    GuideJsonLoader jsonLoader
)
{
    private readonly Dictionary<SupportedServices, ICollection<CustomFormatGroupData>> _cache =
        new();

    private IReadOnlyList<IDirectoryInfo> CreatePaths(SupportedServices serviceType)
    {
        var metadata = metadataBuilder.GetMetadata();
        return serviceType switch
        {
            SupportedServices.Radarr => metadataBuilder.ToDirectoryInfoList(
                metadata.JsonPaths.Radarr.CustomFormatGroups
            ),
            SupportedServices.Sonarr => metadataBuilder.ToDirectoryInfoList(
                metadata.JsonPaths.Sonarr.CustomFormatGroups
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, null),
        };
    }

    public ICollection<CustomFormatGroupData> GetGuideData(SupportedServices serviceType)
    {
        if (_cache.TryGetValue(serviceType, out var cfData))
        {
            return cfData;
        }

        var paths = CreatePaths(SupportedServices.Radarr);
        return _cache[serviceType] = jsonLoader.LoadAllFilesAtPaths<CustomFormatGroupData>(paths);
    }
}
