using HttpServerSim.Client;

namespace HttpServerSim.App.CommandLineArgs.Tests;

public class BaseTest
{
    protected const string TEST_SIM_URL = "http://localhost:5002";
    protected static HttpClient HttpClient { get; } = HttpClientFactory.CreateHttpClient("test-client");
    protected static TimeSpan Timeout {  get; } = TimeSpan.FromSeconds(5);

    public TestContext TestContext { get; set; }
}
