using Recyclarr.Cache;
using Recyclarr.Cli.Pipelines.CustomFormat.Cache;
using Recyclarr.Cli.Pipelines.CustomFormat.Models;
using Recyclarr.Common.Extensions;
using Recyclarr.Config.Models;
using Recyclarr.ServarrApi.CustomFormat;
using Recyclarr.TrashGuide;
using Recyclarr.TrashGuide.CustomFormat;

namespace Recyclarr.Cli.Pipelines.CustomFormat;

internal class CustomFormatPipeline(
    ILogger log,
    IServiceConfiguration config,
    CustomFormatTransactionLogger transactionLogger,
    ICustomFormatGuideService guide,
    ProcessedCustomFormatCache cache,
    ICachePersister<CustomFormatCache> cachePersister,
    ICustomFormatApiService api
) : BaseSyncPipeline<CustomFormatPipelineContext>
{
    public override string Description => "Custom Format";

    public override IReadOnlyCollection<SupportedServices> CompatibleServices { get; } =
        [SupportedServices.Sonarr, SupportedServices.Radarr];

    protected override Task ConfigurePhase(
        CustomFormatPipelineContext context,
        CancellationToken ct
    )
    {
        // Match custom formats in the YAML config to those in the guide, by Trash ID
        //
        // This solution is conservative: CustomFormatData is only created for CFs in the guide that are
        // specified in the config.
        //
        // The ToLookup() at the end finds TrashIDs provided in the config that do not match anything in the guide.
        // These will yield a warning in the logs.
        var processedCfs = config
            .CustomFormats.SelectMany(x => x.TrashIds)
            .Distinct(StringComparer.InvariantCultureIgnoreCase)
            .GroupJoin(
                guide.GetCustomFormatData(config.ServiceType),
                x => x,
                x => x.TrashId,
                (id, cf) => (Id: id, CustomFormats: cf)
            )
            .ToLookup(x => x.Item2.Any());

        context.InvalidFormats = processedCfs[false].Select(x => x.Id).ToList();
        context.ConfigOutput.AddRange(processedCfs[true].SelectMany(x => x.CustomFormats));
        context.Cache = cachePersister.Load();

        cache.AddCustomFormats(context.ConfigOutput);
        return Task.CompletedTask;
    }

    // Returning 'true' means to exit. 'false' means to proceed.
    protected override bool LogConfigPhaseAndExitIfNeeded(CustomFormatPipelineContext context)
    {
        if (context.InvalidFormats.Count != 0)
        {
            log.Warning(
                "These Custom Formats do not exist in the guide and will be skipped: {Cfs}",
                context.InvalidFormats
            );
        }

        // Do not exit when the config has zero custom formats. We still may need to delete old custom formats.
        return false;
    }

    protected override async Task FetchPhase(
        CustomFormatPipelineContext context,
        CancellationToken ct
    )
    {
        var result = await api.GetCustomFormats(ct);
        context.ApiFetchOutput.AddRange(result);
        context.Cache.RemoveStale(result);
    }

    protected override void TransactionPhase(CustomFormatPipelineContext context)
    {
        var transactions = new CustomFormatTransactionData();

        foreach (var guideCf in context.ConfigOutput)
        {
            log.Debug(
                "Process transaction for guide CF {TrashId} ({Name})",
                guideCf.TrashId,
                guideCf.Name
            );

            guideCf.Id = context.Cache.FindId(guideCf) ?? 0;

            var serviceCf = FindServiceCfByName(context.ApiFetchOutput, guideCf.Name);
            if (serviceCf is not null)
            {
                ProcessExistingCf(guideCf, serviceCf, transactions);
                continue;
            }

            serviceCf = FindServiceCfById(context.ApiFetchOutput, guideCf.Id);
            if (serviceCf is not null)
            {
                // We do not use AddUpdatedCustomFormat() here because it's impossible for the CFs to be identical if we
                // got to this point. Reason: We reach this code if the names are not the same. At the very least, this
                // means the name needs to be updated in the service.
                transactions.UpdatedCustomFormats.Add(guideCf);
            }
            else
            {
                transactions.NewCustomFormats.Add(guideCf);
            }
        }

        if (config.DeleteOldCustomFormats)
        {
            transactions.DeletedCustomFormats.AddRange(
                context
                    .Cache.TrashIdMappings
                    // Custom format must be in the cache but NOT in the user's config
                    .Where(map => context.ConfigOutput.All(cf => cf.TrashId != map.TrashId))
                    // Also, that cache-only CF must exist in the service (otherwise there is nothing to delete)
                    .Where(map => context.ApiFetchOutput.Any(cf => cf.Id == map.CustomFormatId))
            );
        }

        context.TransactionOutput = transactions;
    }

    private void ProcessExistingCf(
        CustomFormatData guideCf,
        CustomFormatData serviceCf,
        CustomFormatTransactionData transactions
    )
    {
        if (config.ReplaceExistingCustomFormats)
        {
            // replace:
            // - Use the ID from the service, not the cache, and do an update
            if (guideCf.Id != serviceCf.Id)
            {
                log.Debug(
                    "Format IDs for CF {Name} did not match which indicates a manually-created CF is "
                        + "replaced, or that the cache is out of sync with the service ({GuideId} != {ServiceId})",
                    serviceCf.Name,
                    guideCf.Id,
                    serviceCf.Id
                );

                guideCf.Id = serviceCf.Id;
            }

            AddUpdatedCustomFormat(guideCf, serviceCf, transactions);
        }
        else
        {
            // NO replace:
            // - ids must match (can't rename another cf to the same name), otherwise error
            if (guideCf.Id != serviceCf.Id)
            {
                transactions.ConflictingCustomFormats.Add(
                    new ConflictingCustomFormat(guideCf, serviceCf.Id)
                );
            }
            else
            {
                AddUpdatedCustomFormat(guideCf, serviceCf, transactions);
            }
        }
    }

    private static void AddUpdatedCustomFormat(
        CustomFormatData guideCf,
        CustomFormatData serviceCf,
        CustomFormatTransactionData transactions
    )
    {
        if (guideCf != serviceCf)
        {
            transactions.UpdatedCustomFormats.Add(guideCf);
        }
        else
        {
            transactions.UnchangedCustomFormats.Add(guideCf);
        }
    }

    private static CustomFormatData? FindServiceCfByName(
        IEnumerable<CustomFormatData> serviceCfs,
        string cfName
    )
    {
        return serviceCfs.FirstOrDefault(rcf => cfName.EqualsIgnoreCase(rcf.Name));
    }

    private static CustomFormatData? FindServiceCfById(
        IEnumerable<CustomFormatData> serviceCfs,
        int cfId
    )
    {
        return serviceCfs.FirstOrDefault(rcf => cfId == rcf.Id);
    }

    protected override void LogTransactionNotices(CustomFormatPipelineContext context) { }

    protected override void PreviewPhase(CustomFormatPipelineContext context)
    {
        transactionLogger.LogTransactions(context);
    }

    protected override async Task PersistPhase(
        CustomFormatPipelineContext context,
        CancellationToken ct
    )
    {
        var transactions = context.TransactionOutput;

        foreach (var cf in transactions.NewCustomFormats)
        {
            var response = await api.CreateCustomFormat(cf, ct);
            if (response is not null)
            {
                cf.Id = response.Id;
            }
        }

        foreach (var dto in transactions.UpdatedCustomFormats)
        {
            await api.UpdateCustomFormat(dto, ct);
        }

        foreach (var map in transactions.DeletedCustomFormats)
        {
            await api.DeleteCustomFormat(map.CustomFormatId, ct);
        }

        context.Cache.Update(transactions);
        cachePersister.Save(context.Cache);
    }

    protected override void LogPersistResults(CustomFormatPipelineContext context)
    {
        transactionLogger.LogTransactions(context);
    }
}
