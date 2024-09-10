#nullable disable

using HttpServerSim.Client;
using HttpServerSim.Tests.Shared;
using System.Diagnostics;
using System.Net;

namespace HttpServerSim.App.CommandLineArgs.Tests;

[TestClass]
public class CommandLineArgsTest
{
    private const string TEST_SIM_URL = "http://localhost:5002";
    private const string EXPECTED_HELP_OUTPUT = @"HttpServerSim version: 1.0.0.0
Usage: http-server-sim [options...]
--ControlUrl <url>                 URL for managing rules dynamically. Not required. Example: http://localhost:5001.
--DefaultContentType <value>       The Content-Type used in a response message when no rule matching the request is found.
--DefaultContentValue <value>      The Content used in a response message when no rule matching the request is found.
--DefaultDelay <value>             The delay (in milliseconds) before sending a default response message when no matching rule for the request is found. Default: 0.
--DefaultDelayMax <value>          The maximum delay (in milliseconds) before sending a default response message when no matching rule for the request is found.
                                   When --DefaultDelayMax is specified, the actual delay will be a random value between --DefaultDelay and --DefaultDelayMax.
--DefaultStatusCode <value>        The HTTP status code used in a response message when no rule matching the request is found. Default: 200.
--Help                             Prints this help.
--LogControlRequestAndResponse     Whether control requests and responses are logged. Default: false.
--LogRequestAndResponse            Whether requests and responses are logged. Default: true.
--RequestBodyLogLimit <limit>      Maximum request body size to log (in bytes). Default: 4096.
--ResponseBodyLogLimit <limit>     Maximum response body size to log (in bytes). Default: 4096.
--Rules <file-name> | <path>       Rules file. It can be a file name of a file that exists in the current directory or a full path to a file.
--SaveRequests <directory>         The directory where request messages are saved.
--SaveResponses <directory>        The directory where response messages are saved.
--Url <url>                        URL for simulating endpoints. Default: http://localhost:5000.
                                   --Url and --ControlUrl cannot share the same value.
";

    private static readonly HttpClient _httpClient = HttpClientFactory.CreateHttpClient(nameof(CommandLineArgsTest));
    private HttpServerSimHostTest _host;
    public TestContext TestContext { get; set; }

    [TestCleanup]
    public void Cleanup()
    {
        _host?.FlushLogs();
        _host?.Dispose();
    }

    [TestMethod]
    public void HelpArg()
    {
        var args = "--Help";
        _host = new HttpServerSimHostTest(TestContext, args);

        _host.RunAndWaitForExit(TimeSpan.FromSeconds(10));
        var actualLogs = _host.FlushLogs();

        Assert.IsTrue(actualLogs.Contains(EXPECTED_HELP_OUTPUT), $"Expected log not found.{Environment.NewLine}Expected:{Environment.NewLine}{EXPECTED_HELP_OUTPUT}");
    }

    [TestMethod]
    public async Task Given_args_DefaultDelay_and_DefaultDelayMax_Are_not_present_Should_respond_quickly()
    {
        var args = $"--Url {TEST_SIM_URL}";
        var elapsedMilliseconds = await TimeRequest(args);
        Assert.IsTrue(elapsedMilliseconds < 1000);
    }

    [TestMethod]
    public async Task Given_arg_DefaultDelay_is_two_seconds_Should_respond_after_two_seconds()
    {
        var args = $"--Url {TEST_SIM_URL} --DefaultDelay 2000";
        var elapsedMilliseconds = await TimeRequest(args);
        Assert.IsTrue(elapsedMilliseconds > 2000);
        Assert.IsTrue(elapsedMilliseconds < 2500);
    }

    [TestMethod]
    public async Task Given_arg_DefaultDelay_is_one_second_and_DefaultDelayMax_is_two_seconds_Should_respond_between_one_and_two_seconds()
    {
        var args = $"--Url {TEST_SIM_URL} --DefaultDelay 1000 --DefaultDelayMax 2000";
        var elapsedMilliseconds = await TimeRequest(args);
        Assert.IsTrue(elapsedMilliseconds > 1000);
        Assert.IsTrue(elapsedMilliseconds < 2100);
    }

    private async Task<long> TimeRequest(string args)
    {
        _host = new HttpServerSimHostTest(TestContext, args);

        _host.Start(waitForServiceUsingARequest: false);
        try
        {
            Assert.IsTrue(WaitForLog($"Now listening on: {TEST_SIM_URL}", TimeSpan.FromSeconds(10)), "A log indicating that the service is running was not found.");

            var sw = Stopwatch.StartNew();
            var actualHttpResponse = await _httpClient.GetAsync($"{TEST_SIM_URL}/some-invalid-url-to-get-the-default-response");
            TestContext.WriteLine($"ElapsedMilliseconds: {sw.ElapsedMilliseconds}");
            AssertStatusCode(200, actualHttpResponse.StatusCode);
            return sw.ElapsedMilliseconds;
        }
        finally
        {
            _host.Stop();
        }
    }

    private bool WaitForLog(string log, TimeSpan timeout) => _host.TryFindLog(log);

    private static void AssertStatusCode(int expectedStatusCode, HttpStatusCode actualStatusCode)
    {
        Assert.AreEqual(expectedStatusCode, (int)actualStatusCode);
    }
}