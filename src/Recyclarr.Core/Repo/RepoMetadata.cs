namespace Recyclarr.Repo;

public record RadarrMetadata
{
    public IReadOnlyCollection<string> CustomFormats { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<string> Qualities { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<string> Naming { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<string> CustomFormatGroups { get; init; } = Array.Empty<string>();
}

public record SonarrMetadata
{
    public IReadOnlyCollection<string> Qualities { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<string> CustomFormats { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<string> Naming { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<string> CustomFormatGroups { get; init; } = Array.Empty<string>();
}

public record JsonPaths
{
    public RadarrMetadata Radarr { get; init; } = new();
    public SonarrMetadata Sonarr { get; init; } = new();
}

public record RepoMetadata
{
    public JsonPaths JsonPaths { get; init; } = new();
}
