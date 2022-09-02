namespace TrashLib.Services.Sonarr;

public record SonarrCapabilities(Version Version)
{
    public SonarrCapabilities() : this(new Version())
    {
    }

    public bool SupportsNamedReleaseProfiles { get; init; }

    // Background: Issue #16 filed which points to a backward-breaking API
    // change made in Sonarr at commit [deed85d2f].
    //
    // [deed85d2f]: https://github.com/Sonarr/Sonarr/commit/deed85d2f9147e6180014507ef4f5af3695b0c61
    public bool ArraysNeededForReleaseProfileRequiredAndIgnored { get; init; }

    public bool SupportsCustomFormats { get; init; }
}
