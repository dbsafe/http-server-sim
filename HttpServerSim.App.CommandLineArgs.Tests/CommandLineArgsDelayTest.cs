using HttpServerSim.Tests.Shared;
using System.Diagnostics;
using System.Net;

namespace HttpServerSim.App.CommandLineArgs.Tests;

// TODO: Consolidate all the test that test the command line args in this class/project
[TestClass]
public class CommandLineArgsDelayTest : BaseTest
{
    [TestMethod]
    public async Task Given_args_DefaultDelay_and_DefaultDelayMax_Are_not_present_Should_respond_quickly()
    {
        var args = $"--Url {TEST_SIM_URL}";
        var elapsedMilliseconds = await TimeRequest(args);
        Assert.IsTrue(elapsedMilliseconds < 1000);
    }

    [TestMethod]
    public async Task Given_arg_DefaultDelayMin_is_two_seconds_Should_respond_after_two_seconds()
    {
        var args = $"--Url {TEST_SIM_URL} --DefaultDelayMin 2000";
        var elapsedMilliseconds = await TimeRequest(args);
        Assert.IsTrue(elapsedMilliseconds >= 2000);
        Assert.IsTrue(elapsedMilliseconds < 2500);
    }

    [TestMethod]
    public async Task Given_arg_DefaultDelayMin_is_one_second_and_DefaultDelayMax_is_two_seconds_Should_respond_between_one_and_two_seconds()
    {
        var args = $"--Url {TEST_SIM_URL} --DefaultDelayMin 1000 --DefaultDelayMax 2000";
        var elapsedMilliseconds = await TimeRequest(args);
        Assert.IsTrue(elapsedMilliseconds >= 1000);
        Assert.IsTrue(elapsedMilliseconds < 2100);
    }

    private async Task<long> TimeRequest(string args)
    {
        using var host = new HttpServerSimHostTest(TestContext, args, TEST_SIM_URL);
        var started = false;
        try
        {
            host.Start();
            started = true;
            var sw = Stopwatch.StartNew();
            var actualHttpResponse = await HttpClient.GetAsync($"{TEST_SIM_URL}/some-invalid-url-to-get-the-default-response");
            TestContext.WriteLine($"ElapsedMilliseconds: {sw.ElapsedMilliseconds}");
            AssertStatusCode(200, actualHttpResponse.StatusCode);
            return sw.ElapsedMilliseconds;
        }
        finally
        {
            if (started)
            {
                host.Stop();
            }

            host.FlushLogs();
        }
    }

    private static void AssertStatusCode(int expectedStatusCode, HttpStatusCode actualStatusCode)
    {
        Assert.AreEqual(expectedStatusCode, (int)actualStatusCode);
    }
}