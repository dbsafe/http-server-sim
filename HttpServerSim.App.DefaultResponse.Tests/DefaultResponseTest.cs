#nullable disable

namespace HttpServerSim.App.DefaultResponse.Tests;

[TestClass]
public class DefaultResponseTest
{
    private readonly string _simulatorUrl = AppInitializer.TestHost.SimulatorUrl;
    private static readonly HttpClient _httpClient = AppInitializer.TestHost.HttpClient;
    private string _actualResponseLog;

    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void TestInitialize()
    {
        AppInitializer.TestHost.FlushLogs();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        AppInitializer.TestHost.FlushLogs();
    }

    [TestMethod]
    public async Task Default_response_arguments_should_be_used_in_a_default_response()
    {
        await _httpClient.GetAsync($"{_simulatorUrl}/path-without-a-rule");
        FindRequestLogAndResponseLog();

        var expectedResponseLog = @"Response:
Status Code: 301
Headers:
  Content-Type: text/plain
Body:
moved
End of Response";
        Assert.AreEqual(expectedResponseLog, _actualResponseLog);
    }

    private void FindRequestLogAndResponseLog()
    {
        Assert.IsTrue(AppInitializer.TestHost.TryFindLogSection("Request:", "End of Request", out _), "Request log not found");
        Assert.IsTrue(AppInitializer.TestHost.TryFindLogSection("Response:", "End of Response", out _actualResponseLog), "Response log not found");
    }
}
