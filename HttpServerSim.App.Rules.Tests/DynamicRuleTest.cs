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

    [TestMethod]
    public async Task Given_QueryString_equals_condition_Should_match_url_encoded_request_query_string()
    {
        var rule = CreateQueryStringRule("query-string-equals", Operator.Equals, "from=2026-07-22 15:15:00 -04:00", 204);

        await AssertRequestStatusCodeAsync(rule, "query-string-equals?from=2026-07-22%2015%3A15%3A00%20-04%3A00", 204);
    }

    [TestMethod]
    public async Task Given_QueryString_equals_condition_with_plain_datetime_Should_match_request_query_string()
    {
        var rule = CreateQueryStringRule("query-string-equals-plain-datetime", Operator.Equals, "from=2026-07-22T15:45:00-04:00", 208);

        await AssertRequestStatusCodeAsync(rule, "query-string-equals-plain-datetime?from=2026-07-22T15:45:00-04:00", 208);
    }

    [TestMethod]
    public async Task Given_QueryString_equals_condition_with_plain_datetime_Should_match_url_encoded_datetime()
    {
        var rule = CreateQueryStringRule("query-string-equals-encoded-datetime", Operator.Equals, "from=2026-07-22T15:45:00-04:00", 209);

        await AssertRequestStatusCodeAsync(rule, "query-string-equals-encoded-datetime?from=2026-07-22T15%3A45%3A00-04%3A00", 209);
    }

    [TestMethod]
    public async Task Given_QueryString_start_with_condition_Should_match_request_query_string()
    {
        var rule = CreateQueryStringRule("query-string-starts-with", Operator.StartWith, "from=2026-07-22", 205);

        await AssertRequestStatusCodeAsync(rule, "query-string-starts-with?from=2026-07-22&to=2026-07-23", 205);
    }

    [TestMethod]
    public async Task Given_QueryString_contains_condition_Should_match_request_query_string()
    {
        var rule = CreateQueryStringRule("query-string-contains", Operator.Contains, "to=2026-07-23", 206);

        await AssertRequestStatusCodeAsync(rule, "query-string-contains?from=2026-07-22&to=2026-07-23", 206);
    }

    [TestMethod]
    public async Task Given_Request_has_no_query_string_Should_not_match_query_string_condition()
    {
        var rule = CreateQueryStringRule("query-string-absent", Operator.Contains, "from=2026-07-22", 207);

        await AssertRequestStatusCodeAsync(rule, "query-string-absent", 404);
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

    private static ConfigRule CreateQueryStringRule(string name, Operator @operator, string value, int statusCode) =>
        RuleBuilder.CreateRule(name)
            .WithCondition(field: Field.Path, op: Operator.Contains, value: name)
            .WithCondition(field: Field.QueryString, op: @operator, value: value)
            .ReturnWithStatusCode(statusCode)
            .Rule;

    private async Task AssertRequestStatusCodeAsync(ConfigRule rule, string pathAndQuery, int expectedStatusCode)
    {
        _httpSimClient.AddRule(rule);

        var response = await _httpClient.GetAsync($"{AppInitializer.TEST_SIM_URL}/{pathAndQuery}");

        Assert.AreEqual(expectedStatusCode, (int)response.StatusCode);
    }
}
