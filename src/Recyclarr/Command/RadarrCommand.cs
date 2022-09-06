using CliFx.Attributes;
using JetBrains.Annotations;
using Recyclarr.Config;
using Serilog;
using TrashLib.Extensions;
using TrashLib.Services.CustomFormat;
using TrashLib.Services.Radarr;
using TrashLib.Services.Radarr.Config;
using TrashLib.Services.Radarr.QualityDefinition;

namespace Recyclarr.Command;

[Command("radarr", Description = "Perform operations on a Radarr instance")]
[UsedImplicitly]
internal class RadarrCommand : ServiceCommand
{
    [CommandOption("list-custom-formats", Description =
        "List available custom formats from the guide in YAML format.")]
    public bool ListCustomFormats { get; [UsedImplicitly] set; }

    [CommandOption("list-qualities", Description =
        "List available quality definition types from the guide.")]
    public bool ListQualities { get; [UsedImplicitly] set; }

    public override string Name => "Radarr";

    public override async Task Process(IServiceLocatorProxy container)
    {
        await base.Process(container);

        var lister = container.Resolve<IRadarrGuideDataLister>();
        var log = container.Resolve<ILogger>();
        var customFormatUpdaterFactory = container.Resolve<Func<ICustomFormatUpdater>>();
        var qualityUpdaterFactory = container.Resolve<Func<IRadarrQualityDefinitionUpdater>>();
        var configLoader = container.Resolve<IConfigurationLoader<RadarrConfiguration>>();
        var guideService = container.Resolve<IRadarrGuideService>();

        if (ListCustomFormats)
        {
            lister.ListCustomFormats();
            return;
        }

        if (ListQualities)
        {
            lister.ListQualities();
            return;
        }

        foreach (var config in configLoader.LoadMany(Config, "radarr"))
        {
            log.Information("Processing server {Url}", FlurlLogging.SanitizeUrl(config.BaseUrl));

            if (config.QualityDefinition != null)
            {
                await qualityUpdaterFactory().Process(Preview, config);
            }

            if (config.CustomFormats.Count > 0)
            {
                await customFormatUpdaterFactory().Process(Preview, config.CustomFormats, guideService);
            }
        }
    }
}
