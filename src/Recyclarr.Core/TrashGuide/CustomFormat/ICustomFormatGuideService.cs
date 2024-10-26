namespace Recyclarr.TrashGuide.CustomFormat;

public interface ICustomFormatGuideService
{
    ICollection<CustomFormatData> GetGuideData(SupportedServices serviceType);
}
