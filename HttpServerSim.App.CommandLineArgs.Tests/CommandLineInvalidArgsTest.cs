using HttpServerSim.Tests.Shared;

namespace HttpServerSim.App.CommandLineArgs.Tests;

[TestClass]
public class CommandLineInvalidArgsTest : BaseTest
{
    [TestMethod]
    public void ArgsValidation_Given_invalid_args_Should_print_message()
    {
        var args = "--CurrentDirectory directory --More more Other --DefaultStatusCode 301 --Logging:LogLevel:HttpServerSim Debug";
        using var host = new HttpServerSimHostTest(TestContext, args);

        host.RunAndWaitForExit(Timeout);
        var actualLogs = host.FlushLogs();

        var expectedLogs = $"Invalid options:{Environment.NewLine}\tCurrentDirectory{Environment.NewLine}\tMore{Environment.NewLine}\tOther";

        Assert.IsTrue(actualLogs.Contains(expectedLogs), $"Expected log not found.{Environment.NewLine}Expected:{Environment.NewLine}{expectedLogs}");
    }
}
