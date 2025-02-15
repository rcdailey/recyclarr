using Flurl.Http.Testing;
using Recyclarr.Common;
using Recyclarr.Config;
using Recyclarr.Config.Models;
using Recyclarr.Core.Tests.Reusable;
using Recyclarr.ServarrApi.CustomFormat;
using Recyclarr.TestLibrary;

namespace Recyclarr.Core.Tests.IntegrationTests;

public class CustomFormatServiceTest : IntegrationTestFixture
{
    [Test]
    public async Task Get_can_parse_json()
    {
        var resourceData = new ResourceDataReader(typeof(CustomFormatServiceTest), "Data");
        var jsonBody = resourceData.ReadData("issue_178.json");

        using var http = new HttpTest();
        http.RespondWith(jsonBody);

        var scopeFactory = Resolve<ConfigurationScopeFactory>();
        using var scope = scopeFactory.Start<TestConfigurationScope>(
            new RadarrConfiguration { InstanceName = "instance" }
        );

        var sut = scope.Resolve<CustomFormatApiService>();
        var result = await sut.GetCustomFormats(CancellationToken.None);

        result.Should().HaveCountGreaterThan(5);
    }
}
