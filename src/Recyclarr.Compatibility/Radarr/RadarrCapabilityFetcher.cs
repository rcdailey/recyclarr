namespace Recyclarr.Compatibility.Radarr;

public class RadarrCapabilityFetcher : ServiceCapabilityFetcher<RadarrCapabilities>, IRadarrCapabilityFetcher
{
    public RadarrCapabilityFetcher(IServiceInformation info)
        : base(info)
    {
    }

    protected override RadarrCapabilities BuildCapabilitiesObject(Version? version)
    {
        return new RadarrCapabilities(version);
    }
}