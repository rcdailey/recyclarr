using Flurl.Http;
using Recyclarr.Config.Models;
using Recyclarr.ServarrApi.System;
using Serilog;

namespace Recyclarr.Compatibility;

public class ServiceInformation(ServarrApiServiceFactory apiFactory, ILogger log) : IServiceInformation
{
    public async Task<Version> GetVersion(IServiceConfiguration config)
    {
        try
        {
            var api = apiFactory.Create<ISystemApiService>(config);
            var status = await api.GetStatus();
            log.Debug("{Service} Version: {Version}", status.AppName, status.Version);
            return new Version(status.Version);
        }
        catch (FlurlHttpException)
        {
            log.Error("Unable to obtain service version information");
            throw;
        }
    }
}
