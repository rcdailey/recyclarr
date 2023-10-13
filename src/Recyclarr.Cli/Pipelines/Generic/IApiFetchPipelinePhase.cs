using Recyclarr.Config.Models;

namespace Recyclarr.Cli.Pipelines.Generic;

public interface IApiFetchPipelinePhase<in TContext>
{
    Task Execute(TContext context, IServiceConfiguration config);
}