#nullable disable

using HttpServerSim.Tests.Shared;

namespace HttpServerSim.App.CommandLineArgs.Tests;

[TestClass]
public class CommandLineArgsTest
{
    private static string _expectedHelpOutput = @"HttpServerSim version: 1.0.0.0
Usage: http-server-sim [options...]
--ControlUrl <url>                 URL for managing rules dynamically. Not required. Example: http://localhost:5001.
--DefaultContentType <value>       The Content-Type used in a response message when no rule matching the request is found.
--DefaultContentValue <value>      The Content used in a response message when no rule matching the request is found.
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

    private HttpServerSimHostTest _host;
    public TestContext TestContext { get; set; }

    [TestCleanup]
    public void Cleanup()
    {
        _host?.Dispose();
    }

    [TestMethod]
    public void HelpArg()
    {
        var args = "--Help";
        _host = new HttpServerSimHostTest(TestContext, args);

        _host.RunAndWaitForExit(TimeSpan.FromSeconds(10));
        var actualLogs = _host.FlushLogs();

        Assert.IsTrue(actualLogs.Contains(_expectedHelpOutput), "Expected logs not found.");
    }
}