using Recyclarr.Cli.Pipelines.QualitySize.PipelinePhases.Limits;
using Recyclarr.Core.Tests.Reusable;
using Recyclarr.TrashGuide;
using Recyclarr.TrashGuide.QualitySize;

namespace Recyclarr.Cli.Tests.Reusable;

public class TestQualityItemLimitFactory : IQualityItemLimitFactory
{
    public Task<QualityItemLimits> Create(SupportedServices serviceType, CancellationToken ct)
    {
        return Task.FromResult(
            new QualityItemLimits(
                TestQualityItemLimits.MaxUnlimitedThreshold,
                TestQualityItemLimits.PreferredUnlimitedThreshold
            )
        );
    }
}
