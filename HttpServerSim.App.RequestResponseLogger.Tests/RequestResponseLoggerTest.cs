// Ignore Spelling: json

using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text;

namespace HttpServerSim.App.RequestResponseLogger.Tests;

[TestClass]
public class RequestResponseLoggerTest
{
    private readonly string _simulatorUrl = AppInitializer.SimulatorUrl;
    private static readonly HttpClient _httpClient = AppInitializer.HttpClient;
    private string? _actualRequestLog;
    private string? _actualResponseLog;

    private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);

    [TestInitialize]
    public void TestInitialize()
    {
        FlushLogs();
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

    private void LoadRequestLogAndResponseLog()
    {
        var sb = new StringBuilder();

        Assert.IsTrue(TryFindSection("Request:", "End of Request", sb, _timeout, out _actualRequestLog), "Request log not found");
        Assert.IsTrue(TryFindSection("Response:", "End of Response", sb, _timeout, out _actualResponseLog), "Response log not found");
    }

    private static void FlushLogs()
    {
        while (AppInitializer.LogsQueue.TryDequeue(out var log))
        {
            Console.WriteLine(log);
        }
    }

    private static bool TryFindSection(string startToken, string endToken, StringBuilder logs, TimeSpan timeout, [NotNullWhen(true)] out string? section)
    {
        var expiration = DateTimeOffset.UtcNow + timeout;
        while (expiration > DateTimeOffset.UtcNow)
        {
            while (AppInitializer.LogsQueue.TryDequeue(out var log))
            {
                Console.WriteLine(log);
                logs.AppendLine(log);
            }

            var currentLogs = logs.ToString();
            var startIndex = currentLogs.IndexOf(startToken);
            var endIndex = currentLogs.IndexOf(endToken);

            if (startIndex == -1 || endIndex == -1)
            {
                Thread.Sleep(100);
                continue;
            }

            if (startIndex > endIndex)
            {
                section = null;
                return false;
            }

            section = currentLogs.Substring(startIndex, endIndex - startIndex + endToken.Length);
            return true;
        }

        section = null;
        return false;
    }
}