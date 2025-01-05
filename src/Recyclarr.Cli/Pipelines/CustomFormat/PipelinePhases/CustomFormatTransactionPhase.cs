using Recyclarr.Cli.Pipelines.CustomFormat.Models;
using Recyclarr.Cli.Pipelines.Generic;
using Recyclarr.Common.Extensions;
using Recyclarr.Config.Models;
using Recyclarr.TrashGuide.CustomFormat;

namespace Recyclarr.Cli.Pipelines.CustomFormat.PipelinePhases;

public class CustomFormatTransactionPhase() : ITransactionPipelinePhase<CustomFormatPipelineContext>
{
    public void Execute(CustomFormatPipelineContext context) { }
}
