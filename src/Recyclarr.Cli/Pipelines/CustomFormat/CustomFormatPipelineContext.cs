using Recyclarr.Cli.Pipelines.CustomFormat.Cache;
using Recyclarr.Cli.Pipelines.CustomFormat.Models;
using Recyclarr.TrashGuide.CustomFormat;

namespace Recyclarr.Cli.Pipelines.CustomFormat;

public class CustomFormatPipelineContext
{
    public IList<CustomFormatData> ConfigOutput { get; init; } = [];
    public IList<CustomFormatData> ApiFetchOutput { get; init; } = [];
    public CustomFormatTransactionData TransactionOutput { get; set; } = null!;
    public IReadOnlyCollection<string> InvalidFormats { get; set; } = null!;
    public CustomFormatCache Cache { get; set; } = null!;
}
