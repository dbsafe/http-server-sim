// Ignore Spelling: json

using System.Net.Http.Json;

namespace HttpServerSim.App.RequestResponseLogger.Tests;

[TestClass]
public class ConsoleRequestResponseLoggerPresentationTest
{
    private readonly string _simulatorUrl = AppInitializer.TEST_SIM_URL;
    private readonly string _simulatorHost = AppInitializer.TEST_SIM_HOST;
    private static readonly HttpClient _httpClient = AppInitializer.TestHost.HttpClient;
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(20);

    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void Initialize()
    {
        AppInitializer.Lock();
        AppInitializer.TestHost.FlushLogs();
    }

    [TestCleanup]
    public void Cleanup()
    {
        AppInitializer.TestHost.FlushLogs();
        AppInitializer.Unlock();
    }

    [TestMethod]
    public async Task RequestLog_Simple_request()
    {
        var task = TryFindRequestLog();

        await _httpClient.GetAsync($"{_simulatorUrl}/simple-request");

        var expectedRequestLog = @$"Request:
HTTP/1.1 - GET - {_simulatorUrl}/simple-request
Headers:
  Host: {_simulatorHost}
Body:
[Not present]
End of Request";
        Assert.IsTrue(task.Wait(_timeout));
        Assert.AreEqual(expectedRequestLog, task.Result);
    }

    [TestMethod]
    public async Task RequestLog_Request_with_query_parameters()
    {
        var task = TryFindRequestLog();

        await _httpClient.GetAsync($"{_simulatorUrl}/simple-request&p1=10&p2=Juan");

        var expectedRequestLog = @$"Request:
HTTP/1.1 - GET - {_simulatorUrl}/simple-request&p1=10&p2=Juan
Headers:
  Host: {_simulatorHost}
Body:
[Not present]
End of Request";
        Assert.IsTrue(task.Wait(_timeout));
        Assert.AreEqual(expectedRequestLog, task.Result);
    }

    [TestMethod]
    public async Task RequestLog_Request_with_content()
    {
        var task = TryFindRequestLog();

        var body = new { Id = 1, Name = "name-1" };
        await _httpClient.PostAsJsonAsync($"{_simulatorUrl}/simple-request", body);

        var expectedRequestLog = @$"Request:
HTTP/1.1 - POST - {_simulatorUrl}/simple-request
Headers:
  Host: {_simulatorHost}
  Content-Type: application/json; charset=utf-8
  Transfer-Encoding: chunked
Body:
{{""id"":1,""name"":""name-1""}}
End of Request";
        Assert.IsTrue(task.Wait(_timeout));
        Assert.AreEqual(expectedRequestLog, task.Result);
    }

    [TestMethod]
    public async Task Request_Given_a_request_with_a_body_larger_than_the_limit_Body_should_be_truncated()
    {
        var task = TryFindRequestLog();

        string body = "111111111122222222223333333333444444444455555555556666666666";
        await _httpClient.PostAsJsonAsync($"{_simulatorUrl}/simple-request", body);

        var expectedRequestLog = @$"Request:
HTTP/1.1 - POST - {_simulatorUrl}/simple-request
Headers:
  Host: {_simulatorHost}
  Content-Type: application/json; charset=utf-8
  Transfer-Encoding: chunked
Body:
""11111111112222222222333333333344444444445555555555
[Body truncated. Read 51 characters]
End of Request";
        Assert.IsTrue(task.Wait(_timeout));
        Assert.AreEqual(expectedRequestLog, task.Result);
    }

    [TestMethod]
    public async Task ResponseLog_Simple_response()
    {
        var task = TryFindResponseLog();

        await _httpClient.GetAsync($"{_simulatorUrl}/simple-response");

        var expectedResponseLog = @"Response:
Status Code: 200
Headers:
[Not present]
Body:
[Not present]
End of Response";
        Assert.IsTrue(task.Wait(_timeout));
        Assert.AreEqual(expectedResponseLog, task.Result);
    }

    [TestMethod]
    public async Task ResponseLog_Response_with_headers()
    {
        var task = TryFindResponseLog();

        await _httpClient.GetAsync($"{_simulatorUrl}/response-with-headers");

        var expectedResponseLog = @"Response:
Status Code: 200
Headers:
  header-1: value-1
  header-2: value-21,value-22
Body:
[Not present]
End of Response";
        Assert.IsTrue(task.Wait(_timeout));
        Assert.AreEqual(expectedResponseLog, task.Result);
    }

    [TestMethod]
    public async Task ResponseLog_Response_with_json_content()
    {
        var task = TryFindResponseLog();

        await _httpClient.GetAsync($"{_simulatorUrl}/response-with-json-content");

        var expectedResponseLog = @"Response:
Status Code: 200
Headers:
  Content-Type: application/json
Body:
{""name"":""Juan""}
End of Response";
        Assert.IsTrue(task.Wait(_timeout));
        Assert.AreEqual(expectedResponseLog, task.Result);
    }

    [TestMethod]
    public async Task ResponseLog_Given_a_response_with_a_body_larger_than_the_limit_Body_should_be_truncated()
    {
        var task = TryFindResponseLog();

        await _httpClient.GetAsync($"{_simulatorUrl}/get-response-with-large-json-content");

        var expectedResponseLog = @"Response:
Status Code: 200
Headers:
  Content-Type: application/json
Body:
""111111111122222222223333333333444444444455555555556
[Body truncated. Read 52 characters]
End of Response";
        Assert.IsTrue(task.Wait(_timeout));
        Assert.AreEqual(expectedResponseLog, task.Result);
    }

    private static Task<string> TryFindRequestLog() => AppInitializer.TestHost.TryFindLogSection("Request:", "End of Request");

    private static Task<string> TryFindResponseLog() => AppInitializer.TestHost.TryFindLogSection("Response:", "End of Response");
}
