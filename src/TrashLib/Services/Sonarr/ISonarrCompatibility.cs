namespace TrashLib.Services.Sonarr;

public interface ISonarrCompatibility
{
    IObservable<SonarrCapabilities> Capabilities { get; }
    Version MinimumVersion { get; }
}
