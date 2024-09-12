using HttpServerSim.Client;
using HttpServerSim.Client.Models;
using System.Diagnostics;

namespace HttpServerSim.App.Rules.Tests;

// TODO: Consolidate all the test that test rules being passed in this class/project
// TODO: Add tests to use rules from a file
[TestClass]
public class DynamicRuleTest
{
    private static readonly HttpClient _httpClient = HttpClientFactory.CreateHttpClient(nameof(DynamicRuleTest));
    private static readonly HttpSimClient _httpSimClient = new(AppInitializer.TEST_SIM_CONTROL_URL);

    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void Initialize()
    {
        AppInitializer.Lock();
        _httpSimClient.ClearRules();
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
        var rule = RuleBuilder.CreateRule("rule-without-delay")
            .WithCondition(field: Field.Path, op: Operator.Contains, value: "rule-without-delay")
            .ReturnWithStatusCode(201)
            .Rule;

        var elapsedMilliseconds = await TimeRequestAsync(rule, "rule-without-delay", 201);

        Assert.IsTrue(elapsedMilliseconds < 200);
    }

    [TestMethod]
    public async Task Given_DelayMin_is_two_seconds_Should_respond_after_two_seconds()
    {
        var rule = RuleBuilder.CreateRule("rule-with-delay-min")
            .WithCondition(field: Field.Path, op: Operator.Contains, value: "rule-with-delay-min")
            .ReturnWithStatusCode(202)
            .WithDelay(2000)
            .Rule;

        var elapsedMilliseconds = await TimeRequestAsync(rule, "rule-with-delay-min", 202);

        Assert.IsTrue(elapsedMilliseconds >= 2000);
        Assert.IsTrue(elapsedMilliseconds < 2500);
    }

    [TestMethod]
    public async Task Given_DelayMin_is_one_second_and_DelayMax_is_two_seconds_Should_respond_between_one_and_two_seconds()
    {
        var rule = RuleBuilder.CreateRule("rule-with-delay-min-and-delay-max")
            .WithCondition(field: Field.Path, op: Operator.Contains, value: "rule-with-delay-min-and-delay-max")
            .ReturnWithStatusCode(203)
            .WithDelay(1000, 2000)
            .Rule;

        var elapsedMilliseconds = await TimeRequestAsync(rule, "rule-with-delay-min-and-delay-max", 203);
        Assert.IsTrue(elapsedMilliseconds >= 1000);
        Assert.IsTrue(elapsedMilliseconds < 2100);
    }

    private async Task<long> TimeRequestAsync(ConfigRule rule, string path, int expectedStatusCode)
    {
        _httpSimClient.AddRule(rule);

        var sw = Stopwatch.StartNew();
        var actualHttpResponse = await _httpClient.GetAsync($"{AppInitializer.TEST_SIM_URL}/{path}");
        TestContext.WriteLine($"ElapsedMilliseconds: {sw.ElapsedMilliseconds}");
        Assert.AreEqual(expectedStatusCode, (int)actualHttpResponse.StatusCode);

        return sw.ElapsedMilliseconds;
    }
}