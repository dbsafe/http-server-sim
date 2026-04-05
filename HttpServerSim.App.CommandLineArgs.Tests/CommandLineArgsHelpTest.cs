using HttpServerSim.App.Config;
using HttpServerSim.Tests.Shared;

namespace HttpServerSim.App.CommandLineArgs.Tests;

[TestClass]
public class CommandLineArgsHelpTest : BaseTest
{
    private static string ExpectedVersionLine =>
        $"HttpServerSim version: {typeof(AppConfigLoader).Assembly.GetName().Version}";

    private static string ExpectedHelpOutput => AppConfigLoader.GetHelpText();

    [TestMethod]
    public void Given_arg_Help_is_passed_Should_print_the_help()
    {
        var args = "--Help";
        using var host = new HttpServerSimHostTest(TestContext, args);

        host.RunAndWaitForExit(Timeout);
        var actualLogs = host.FlushLogs();

        AssertHelpIsPrinted(actualLogs);
    }

    [TestMethod]
    public void Help_output_should_include_all_supported_options()
    {
        var args = "--Help";
        using var host = new HttpServerSimHostTest(TestContext, args);

        host.RunAndWaitForExit(Timeout);
        var actualLogs = host.FlushLogs();

        foreach (var validArg in AppConfigLoader.ValidArgs.Keys.Order())
        {
            Assert.IsTrue(actualLogs.Contains($"--{validArg}"), $"Expected help to include option '--{validArg}'.");
        }
    }

    [TestMethod]
    public void Given_invalid_args_Should_print_the_help_after_the_error_message()
    {
        var args = "--CurrentDirectory directory --More more Other --DefaultStatusCode 301 --Logging:LogLevel:HttpServerSim Debug";
        using var host = new HttpServerSimHostTest(TestContext, args);

        host.RunAndWaitForExit(Timeout);
        var actualLogs = host.FlushLogs();

        var expectedInvalidOptionsMessage =
            $"Invalid options:{Environment.NewLine}\tCurrentDirectory{Environment.NewLine}\tMore{Environment.NewLine}\tOther";

        Assert.IsTrue(actualLogs.Contains(expectedInvalidOptionsMessage), $"Expected log not found.{Environment.NewLine}Expected:{Environment.NewLine}{expectedInvalidOptionsMessage}");
        AssertHelpIsPrinted(actualLogs);
    }

    private static void AssertHelpIsPrinted(string actualLogs)
    {
        Assert.IsTrue(
            actualLogs.Contains(ExpectedVersionLine),
            $"Expected version line not found.{Environment.NewLine}Expected:{Environment.NewLine}{ExpectedVersionLine}");

        Assert.IsTrue(
            actualLogs.Contains(ExpectedHelpOutput),
            $"Expected help not found.{Environment.NewLine}Expected:{Environment.NewLine}{ExpectedHelpOutput}");
    }
}
