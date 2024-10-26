using Recyclarr.TrashGuide.MediaNaming;

namespace Recyclarr.TrashGuide.CustomFormatGroups;

public interface IMediaNamingGuideService
{
    RadarrMediaNamingData GetRadarrNamingData();
    SonarrMediaNamingData GetSonarrNamingData();
}
