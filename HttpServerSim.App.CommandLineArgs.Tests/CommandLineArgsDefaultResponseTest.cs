using HttpServerSim.Client;
using HttpServerSim.Tests.Shared;

namespace HttpServerSim.App.CommandLineArgs.Tests;

[TestClass]
public class CommandLineArgsDefaultResponseTest : BaseTest
{
    private static readonly HttpClient _httpClient = HttpClientFactory.CreateHttpClient(nameof(CommandLineArgsDelayTest));

    [TestMethod]
    public async Task Default_response_arguments_should_be_used_in_a_default_response()
    {
        var args = $"--Url {TEST_SIM_URL} --DefaultContentValue moved --DefaultContentType text/plain --DefaultStatusCode 301 --Logging:LogLevel:HttpServerSim Debug";
        using var host = new HttpServerSimHostTest(TestContext, args, TEST_SIM_URL);
        var started = false;
        try
        {
            host.Start();

            var task = host.TryFindLogSection("Response:", "End of Response");
            await _httpClient.GetAsync($"{TEST_SIM_URL}/path-without-a-rule");

            Assert.IsTrue(task.Wait(Timeout));

            var expectedResponseLog = @"Response:
Status Code: 301
Headers:
  Content-Type: text/plain
Body:
moved
End of Response";
            Assert.AreEqual(expectedResponseLog, task.Result);
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
}
