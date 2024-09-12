using HttpServerSim.Client;
using System.Diagnostics;

namespace HttpServerSim.App.StaticRules.Tests;

[TestClass]
public class StaticRuleTest
{
    private static readonly HttpClient _httpClient = HttpClientFactory.CreateHttpClient(nameof(StaticRuleTest));
    private static readonly HttpSimClient _httpSimClient = new(AppInitializer.TEST_SIM_CONTROL_URL);

    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void Initialize()
    {
        AppInitializer.Lock();
    }

    [TestCleanup]
    public void Cleanup()
    {
        AppInitializer.TestHost.FlushLogs();
        AppInitializer.Unlock();
    }

    [TestMethod]
    public async Task Given_Delay_is_not_present_in_a_rule_Should_respond_quickly()
    {
        var elapsedMilliseconds = await TimeRequestAsync("201", 201);

        Assert.IsTrue(elapsedMilliseconds < 200);
    }

    [TestMethod]
    public async Task Given_DelayMin_is_two_seconds_Should_respond_after_two_seconds()
    {
        var elapsedMilliseconds = await TimeRequestAsync("202", 202);

        Assert.IsTrue(elapsedMilliseconds >= 2000);
        Assert.IsTrue(elapsedMilliseconds < 2500);
    }

    [TestMethod]
    public async Task Given_DelayMin_is_one_second_and_DelayMax_is_two_seconds_Should_respond_between_one_and_two_seconds()
    {
        var elapsedMilliseconds = await TimeRequestAsync("203", 203);
        Assert.IsTrue(elapsedMilliseconds >= 1000);
        Assert.IsTrue(elapsedMilliseconds < 2100);
    }

    private async Task<long> TimeRequestAsync(string path, int expectedStatusCode)
    {
        var sw = Stopwatch.StartNew();
        var actualHttpResponse = await _httpClient.GetAsync($"{AppInitializer.TEST_SIM_URL}/{path}");
        TestContext.WriteLine($"ElapsedMilliseconds: {sw.ElapsedMilliseconds}");
        Assert.AreEqual(expectedStatusCode, (int)actualHttpResponse.StatusCode);

        return sw.ElapsedMilliseconds;
    }
}