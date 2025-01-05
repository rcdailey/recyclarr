using Recyclarr.Cli.Pipelines.Generic;

namespace Recyclarr.Cli.Pipelines.CustomFormat.PipelinePhases;

internal class CustomFormatLogPhase(CustomFormatTransactionLogger cfLogger, ILogger log)
    : ILogPipelinePhase<CustomFormatPipelineContext>
{
    public bool LogConfigPhaseAndExitIfNeeded(CustomFormatPipelineContext context) { }

    public void LogTransactionNotices(CustomFormatPipelineContext context) { }

    public void LogPersistenceResults(CustomFormatPipelineContext context) { }
}
