using Recyclarr.Config.Models;

namespace Recyclarr.Core.Tests.Reusable;

public static class NewConfig
{
    public static RadarrConfiguration Radarr()
    {
        return new RadarrConfiguration { InstanceName = "" };
    }

    public static SonarrConfiguration Sonarr()
    {
        return new SonarrConfiguration { InstanceName = "" };
    }
}
