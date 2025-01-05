using Recyclarr.Cli.Console.Settings;
using Recyclarr.TrashGuide;

namespace Recyclarr.Cli.Pipelines;

// This interface is valuable because it allows having a collection of generic sync pipelines without needing to be
// aware of the generic parameters.
public interface ISyncPipeline
{
    string Description { get; }
    IReadOnlyCollection<SupportedServices> CompatibleServices { get; }
    Task Execute(ISyncSettings settings, CancellationToken ct);
}
