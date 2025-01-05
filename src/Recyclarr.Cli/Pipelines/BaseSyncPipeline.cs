using Recyclarr.Cli.Console.Settings;
using Recyclarr.Cli.Pipelines.Generic;
using Recyclarr.TrashGuide;

namespace Recyclarr.Cli.Pipelines;

public abstract class BaseSyncPipeline<TContext> : ISyncPipeline
    where TContext : new()
{
    public abstract string Description { get; }
    public abstract IReadOnlyCollection<SupportedServices> CompatibleServices { get; }

    public async Task Execute(ISyncSettings settings, CancellationToken ct)
    {
        var context = new TContext();
        await Execute(context, settings, ct);
    }

    public async Task Execute(TContext context, ISyncSettings settings, CancellationToken ct)
    {
        await ConfigurePhase(context, ct);
        if (LogConfigPhaseAndExitIfNeeded(context))
        {
            return;
        }

        await FetchPhase(context, ct);
        TransactionPhase(context);

        LogTransactionNotices(context);

        if (settings.Preview)
        {
            PreviewPhase(context);
            return;
        }

        await PersistPhase(context, ct);
        LogPersistResults(context);
    }

    protected abstract Task ConfigurePhase(TContext context, CancellationToken ct);
    protected abstract bool LogConfigPhaseAndExitIfNeeded(TContext context);
    protected abstract Task FetchPhase(TContext context, CancellationToken ct);
    protected abstract void TransactionPhase(TContext context);
    protected abstract void LogTransactionNotices(TContext context);
    protected abstract void PreviewPhase(TContext context);
    protected abstract Task PersistPhase(TContext context, CancellationToken ct);
    protected abstract void LogPersistResults(TContext context);
}
