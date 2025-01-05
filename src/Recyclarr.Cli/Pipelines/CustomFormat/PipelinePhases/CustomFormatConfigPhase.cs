using Recyclarr.Cache;
using Recyclarr.Cli.Pipelines.CustomFormat.Cache;
using Recyclarr.Cli.Pipelines.CustomFormat.Models;
using Recyclarr.Cli.Pipelines.Generic;
using Recyclarr.Common.Extensions;
using Recyclarr.Config.Models;
using Recyclarr.TrashGuide.CustomFormat;

namespace Recyclarr.Cli.Pipelines.CustomFormat.PipelinePhases;

public class CustomFormatConfigPhase(
    ICustomFormatGuideService guide,
    ProcessedCustomFormatCache cache,
    ICachePersister<CustomFormatCache> cachePersister,
    IServiceConfiguration config
) : IConfigPipelinePhase<CustomFormatPipelineContext>
{
    public Task Execute(CustomFormatPipelineContext context, CancellationToken ct) { }
}
