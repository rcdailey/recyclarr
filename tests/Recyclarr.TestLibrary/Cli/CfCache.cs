using Recyclarr.Cli.Pipelines.CustomFormat.Cache;

namespace Recyclarr.TestLibrary.Cli;

public static class CfCache
{
    public static CustomFormatCache New(params TrashIdMapping[] mappings)
    {
        return new CustomFormatCache(
            new CustomFormatCacheObject { TrashIdMappings = mappings.ToList() }
        );
    }
}
