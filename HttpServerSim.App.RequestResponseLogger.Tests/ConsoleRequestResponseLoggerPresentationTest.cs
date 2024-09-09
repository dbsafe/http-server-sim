#nullable disable

// Ignore Spelling: json

using System.Net.Http.Json;

namespace HttpServerSim.App.RequestResponseLogger.Tests;

[TestClass]
public class ConsoleRequestResponseLoggerPresentationTest
{
    private readonly string _simulatorUrl = AppInitializer.TestHost.SimulatorUrl;
    private static readonly HttpClient _httpClient = AppInitializer.TestHost.HttpClient;
    private string _actualRequestLog;
    private string _actualResponseLog;

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
        await _httpClient.GetAsync($"{_simulatorUrl}/simple-request");
        LoadRequestLogAndResponseLog();

        var expectedRequestLog = @"Request:
HTTP/1.1 - GET - http://localhost:5000/simple-request
Headers:
  Host: localhost:5000
Body:
[Not present]
End of Request";
        Assert.AreEqual(expectedRequestLog, _actualRequestLog);
    }

    [TestMethod]
    public async Task RequestLog_Request_with_query_parameters()
    {
        await _httpClient.GetAsync($"{_simulatorUrl}/simple-request&p1=10&p2=Juan");
        LoadRequestLogAndResponseLog();

        var expectedRequestLog = @"Request:
HTTP/1.1 - GET - http://localhost:5000/simple-request&p1=10&p2=Juan
Headers:
  Host: localhost:5000
Body:
[Not present]
End of Request";
        Assert.AreEqual(expectedRequestLog, _actualRequestLog);
    }

    [TestMethod]
    public async Task RequestLog_Request_with_content()
    {
        var body = new { Id = 1, Name = "name-1" };

        await _httpClient.PostAsJsonAsync($"{_simulatorUrl}/simple-request", body);

        LoadRequestLogAndResponseLog();

        var expectedRequestLog = @"Request:
HTTP/1.1 - POST - http://localhost:5000/simple-request
Headers:
  Host: localhost:5000
  Content-Type: application/json; charset=utf-8
  Transfer-Encoding: chunked
Body:
{""id"":1,""name"":""name-1""}
End of Request";
        Assert.AreEqual(expectedRequestLog, _actualRequestLog);
    }

    [TestMethod]
    public async Task Request_Given_a_request_with_a_body_larger_than_the_limit_Body_should_be_truncated()
    {
        string body = "111111111122222222223333333333444444444455555555556666666666";
        await _httpClient.PostAsJsonAsync($"{_simulatorUrl}/simple-request", body);

        LoadRequestLogAndResponseLog();

        var expectedRequestLog = @"Request:
HTTP/1.1 - POST - http://localhost:5000/simple-request
Headers:
  Host: localhost:5000
  Content-Type: application/json; charset=utf-8
  Transfer-Encoding: chunked
Body:
""11111111112222222222333333333344444444445555555555
[Body truncated. Read 51 characters]
End of Request";
        Assert.AreEqual(expectedRequestLog, _actualRequestLog);
    }

    [TestMethod]
    public async Task ResponseLog_Simple_response()
    {
        await _httpClient.GetAsync($"{_simulatorUrl}/simple-response");
        LoadRequestLogAndResponseLog();

        var expectedResponseLog = @"Response:
Status Code: 200
Headers:
[Not present]
Body:
[Not present]
End of Response";
        Assert.AreEqual(expectedResponseLog, _actualResponseLog);
    }

    [TestMethod]
    public async Task ResponseLog_Response_with_headers()
    {
        await _httpClient.GetAsync($"{_simulatorUrl}/response-with-headers");
        LoadRequestLogAndResponseLog();

        var expectedResponseLog = @"Response:
Status Code: 200
Headers:
  header-1: value-1
  header-2: value-21,value-22
Body:
[Not present]
End of Response";
        Assert.AreEqual(expectedResponseLog, _actualResponseLog);
    }

    [TestMethod]
    public async Task ResponseLog_Response_with_json_content()
    {
        await _httpClient.GetAsync($"{_simulatorUrl}/response-with-json-content");
        LoadRequestLogAndResponseLog();

        var expectedResponseLog = @"Response:
Status Code: 200
Headers:
  Content-Type: application/json
Body:
{""name"":""Juan""}
End of Response";
        Assert.AreEqual(expectedResponseLog, _actualResponseLog);
    }

    [TestMethod]
    public async Task ResponseLog_Given_a_response_with_a_body_larger_than_the_limit_Body_should_be_truncated()
    {
        await _httpClient.GetAsync($"{_simulatorUrl}/get-response-with-large-json-content");
        LoadRequestLogAndResponseLog();

        var expectedResponseLog = @"Response:
Status Code: 200
Headers:
  Content-Type: application/json
Body:
""111111111122222222223333333333444444444455555555556
[Body truncated. Read 52 characters]
End of Response";
        Assert.AreEqual(expectedResponseLog, _actualResponseLog);
    }

    private void LoadRequestLogAndResponseLog()
    {
        Assert.IsTrue(AppInitializer.TestHost.TryFindSection("Request:", "End of Request", out _actualRequestLog), "Request log not found");
        Assert.IsTrue(AppInitializer.TestHost.TryFindSection("Response:", "End of Response", out _actualResponseLog), "Response log not found");
    }
}
