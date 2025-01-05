using Recyclarr.Cli.Console.Settings;
using Recyclarr.Compatibility;
using Recyclarr.Config.Models;

namespace Recyclarr.Cli.Pipelines;

public class PipelineExecutor(
    ILogger log,
    IOrderedEnumerable<ISyncPipeline> pipelines,
    IEnumerable<IPipelineCache> caches,
    ServiceAgnosticCapabilityEnforcer enforcer,
    IServiceConfiguration config
)
{
    public async Task Execute(ISyncSettings settings, CancellationToken ct)
    {
        log.Debug("Processing {Server} server {Name}", config.ServiceType, config.InstanceName);

        await enforcer.Check(config, ct);

        foreach (var cache in caches)
        {
            cache.Clear();
        }

        foreach (var pipeline in pipelines)
        {
            log.Debug("Executing Pipeline: {Pipeline}", pipeline.Description);

            if (!pipeline.CompatibleServices.Contains(config.ServiceType))
            {
                log.Debug(
                    "Skipping this pipeline because it does not support service type {Service}",
                    config.ServiceType
                );

                return;
            }

            await pipeline.Execute(settings, ct);
        }

        log.Information("Completed at {Date}", DateTime.Now);
    }
}
