namespace Recyclarr.TrashGuide.CustomFormatGroups;

public record CustomFormatGroupData
{
    public string Name { get; init; }
    public string TrashId { get; init; }
    public List<CustomFormatGroupCfData> CustomFormats { get; init; }
    public CustomFormatGroupQpData? QualityProfiles { get; init; }
}

public record CustomFormatGroupCfData
{
    public string Name { get; init; }
    public string TrashId { get; init; }
    public bool Required { get; init; }
}

public record CustomFormatGroupQpData
{
    public Dictionary<string, string> Exclude { get; init; }
}
