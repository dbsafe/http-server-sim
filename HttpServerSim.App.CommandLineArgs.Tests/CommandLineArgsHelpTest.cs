using HttpServerSim.Tests.Shared;

namespace HttpServerSim.App.CommandLineArgs.Tests;

[TestClass]
public class CommandLineArgsHelpTest : BaseTest
{
    private const string EXPECTED_HELP_OUTPUT = @"HttpServerSim version: 1.0.0.0
Usage: http-server-sim [options...]
--ControlUrl <url>                 URL for managing rules dynamically. Not required. Example: http://localhost:5001.
--DefaultContentType <value>       The Content-Type used in a response message when no rule matching the request is found.
--DefaultContentValue <value>      The Content used in a response message when no rule matching the request is found.
--DefaultDelayMin <value>          The delay (in milliseconds) before sending a default response message when no matching rule for the request is found. Default: 0.
--DefaultDelayMax <value>          The maximum delay (in milliseconds) before sending a default response message when no matching rule for the request is found.
                                   When --DefaultDelayMax is specified, the actual delay will be a random value between --DefaultDelayMin and --DefaultDelayMax.
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

    [TestMethod]
    public void Given_arg_Help_is_passed_Should_print_the_help()
    {
        var args = "--Help";
        using var host = new HttpServerSimHostTest(TestContext, args);

        host.RunAndWaitForExit(Timeout);
        var actualLogs = host.FlushLogs();

        Assert.IsTrue(actualLogs.Contains(EXPECTED_HELP_OUTPUT), $"Expected log not found.{Environment.NewLine}Expected:{Environment.NewLine}{EXPECTED_HELP_OUTPUT}");
    }
}
