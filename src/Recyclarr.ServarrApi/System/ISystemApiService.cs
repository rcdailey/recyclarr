using System.Diagnostics.CodeAnalysis;
using Flurl;
using Recyclarr.Config.Models;
using Recyclarr.Json;
using Refit;

namespace Recyclarr.ServarrApi.System;

public interface ISystemApiService
{
    [Get("/system/status")]
    Task<SystemStatus> GetStatus();
}

public class ServarrApiServiceFactory(IHttpClientFactory clientFactory)
{
    // private readonly SocketsHttpHandler _clientHandler = new();
    // private readonly Dictionary<string, HttpClient> _clientCache = new();
    //
    // private HttpClient MakeHttpClient(IServiceConfiguration config)
    // {
    //     // ReSharper disable once InvertIf
    //     if (!_clientCache.TryGetValue(config.InstanceName, out var client))
    //     {
    //         client = new HttpClient(new ServarrApiAuthHandler(config, _clientHandler))
    //         {
    //             BaseAddress = config.BaseUrl.AppendPathSegments("api", "v3").ToUri()
    //         };
    //
    //         _clientCache.Add(config.InstanceName, client);
    //     }
    //
    //     return client;
    // }

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
    public TService Create<TService>(IServiceConfiguration config)
    {
        var client = clientFactory.CreateClient();
        client.BaseAddress = config.BaseUrl.AppendPathSegments("api", "v3").ToUri();

        // var client = new HttpClient(new ServarrApiAuthHandler(config, new HttpClientHandler()))
        // {
        //     BaseAddress = config.BaseUrl.AppendPathSegments("api", "v3").ToUri()
        // };

        var settings = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(GlobalJsonSerializerSettings.Services)
        };

        return RestService.For<TService>(client, settings);
    }
}

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class ServarrApiAuthHandler(IServiceConfiguration config, HttpMessageHandler innerHandler)
    : DelegatingHandler(innerHandler)
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Add("X-Api-Key", config.ApiKey);
        return await base.SendAsync(request, cancellationToken);
    }
}
