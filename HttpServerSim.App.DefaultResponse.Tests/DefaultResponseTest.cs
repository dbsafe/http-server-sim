using System.Text;

namespace HttpServerSim.App.DefaultResponse.Tests;

[TestClass]
public class DefaultResponseTest
{
    private readonly string _simulatorUrl = AppInitializer.SimulatorUrl;
    private static readonly HttpClient _httpClient = AppInitializer.HttpClient;
    private string? _actualResponseLog;

    [TestInitialize]
    public void TestInitialize()
    {
        AppInitializer.FlushLogs();
    }

    [TestMethod]
    public async Task Default_response_arguments_should_be_used_in_a_default_response()
    {
        await _httpClient.GetAsync($"{_simulatorUrl}/path-without-a-rule");
        LoadRequestLogAndResponseLog();

        var expectedResponseLog = @"Response:
Status Code: 301
Headers:
  Content-Type: text/plain
Body:
moved
End of Response";
        Assert.AreEqual(expectedResponseLog, _actualResponseLog);
    }

    private void LoadRequestLogAndResponseLog()
    {
        var sb = new StringBuilder();

        Assert.IsTrue(AppInitializer.TryFindSection("Request:", "End of Request", sb, out _), "Request log not found");
        Assert.IsTrue(AppInitializer.TryFindSection("Response:", "End of Response", sb, out _actualResponseLog), "Response log not found");
    }
}
