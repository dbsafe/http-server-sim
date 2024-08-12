// Ignore Spelling: json

# nullable disable

using HttpServerSim.Client;
using HttpServerSim.Models;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

namespace EndpointToCSV.Tests;

[TestClass]
public class IntegrationTest3
{
    private static readonly string testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new Exception("Null is not expected here.");
    private static readonly string expectedPathToCSVFile = Path.Combine(testDirectory, "customers.csv");

    private readonly ConcurrentQueue<string> endpointToCSVLogsQueue = new();
    private readonly ConcurrentQueue<string> httpServerSimLogsQueue = new();

    private HttpServerSimHost testHost;
    private static readonly string simulatorUrl = "http://localhost:5000";
    private static readonly string controlUrl = "http://localhost:5001";

    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void Initialize()
    {
        if (File.Exists(expectedPathToCSVFile))
        {
            File.Delete(expectedPathToCSVFile);
        }

        testHost = new HttpServerSimHost(simulatorUrl, testDirectory, $"http-server-sim", $"--ControlUrl {controlUrl} --DefaultStatusCode 404 --Logging:LogLevel:HttpServerSim Debug --Logging:LogLevel:Microsoft.AspNetCore Warning");
        testHost.LogReceived += TestHost_LogReceived;
        testHost.Start();
    }

    [TestCleanup]
    public void Cleanup()
    {
        FlushLogs("EndpointToCSV", endpointToCSVLogsQueue);
        FlushLogs("http-server-sim", httpServerSimLogsQueue);

        testHost?.Stop();
        testHost?.Dispose();
    }

    [TestMethod]
    public void Given_a_json_response_Should_create_a_csv_file()
    {
        // At this point http-server-sim must be running started by the test and the ControlEndpoint listening.
        // And http-server-sim did not load a rules file.

        var httpSimClient = new HttpSimClient(controlUrl);

        // Delete rules that may be created by other tests
        httpSimClient.ClearRules();

        var conditionMethodEqualsGet = new ConfigCondition { Field = Field.Method, Operator = Operator.Equals, Value = "GET" };
        var conditionPathContainsCustomers = new ConfigCondition { Field = Field.Path, Operator = Operator.Contains, Value = "/customers" };

        var customersJson = @"[
  {
    ""id"": ""DD37Cf93aecA6Dc"",
    ""firstName"": ""Sheryl"",
    ""lastName"": ""Baxter"",
    ""company"": ""Rasmussen Group"",
    ""city"": ""East Leonard"",
    ""country"": ""Chile"",
    ""phone1"": ""229-077-5154"",
    ""phone2"": ""397-884-0519x718"",
    ""email"": ""zunigavanessa@smith.info"",
    ""subscriptionDate"": ""2020-08-24"",
    ""website"": ""http://www.stephenson.com""
  }
]";

        var getCustomerRule = RuleBuilder.CreateRule("d-get-customers")
            .WithConditions([conditionMethodEqualsGet, conditionPathContainsCustomers])
            .WithResponse(new HttpSimResponse { StatusCode = 200, ContentType = "application/json", ContentValue = customersJson })
            .Rule;

        // Create a rule dynamically
        httpSimClient.AddRule(getCustomerRule);

        // At this point a dynamic rule was created in http-server-sim to respond with an array of one customer.

        // Executing EndpointToCSV
        using var consoleAppRunner = new ConsoleAppRunner(testDirectory, "dotnet", "EndpointToCSV.dll http://localhost:5000/customers customers.csv");
        consoleAppRunner.OutputDataReceived += ConsoleAppRunner_OutputDataReceived;
        consoleAppRunner.ErrorDataReceived += ConsoleAppRunner_OutputDataReceived;
        var completed = consoleAppRunner.RunWaitForExit(TimeSpan.FromSeconds(10));
        Assert.IsTrue(completed, "Process did not complete on time.");

        // At this point EndpointToCSV completed executing:
        // http-server-sim's output should contain the request and response.
        // File customers.csv in the test directory should contain the correct CVS lines.

        Assert.IsTrue(File.Exists(expectedPathToCSVFile), $"Expected output file '{expectedPathToCSVFile}' not found");

        var actualCSVFileContent = File.ReadAllText(expectedPathToCSVFile);
        TestContext.WriteLine($"Content of file: {expectedPathToCSVFile}{Environment.NewLine}{actualCSVFileContent}");
        TestContext.WriteLine("");

        // This time the file is expected to have one row because the response form calling the endpoint has one item.
        var expectedCSVFileContent = @"Id,FirstName,LastName,Company,City,Country,Phone1,Phone2,Email,SubscriptionDate,Website
DD37Cf93aecA6Dc,Sheryl,Baxter,Rasmussen Group,East Leonard,Chile,229-077-5154,397-884-0519x718,zunigavanessa@smith.info,8/24/2020 12:00:00 AM,http://www.stephenson.com";

        Assert.AreEqual(expectedCSVFileContent, actualCSVFileContent);

        // Request/response can be visually inspected in the test logs.

        // Verify that the method was called one time by verifying that the rule was used once
        httpSimClient.VerifyThatRuleWasUsed(getCustomerRule.Name, 1);
    }

    private void TestHost_LogReceived(object sender, ConsoleAppRunnerEventArgs e) => httpServerSimLogsQueue.Enqueue(e.Data);

    private void ConsoleAppRunner_OutputDataReceived(object sender, ConsoleAppRunnerEventArgs e) => endpointToCSVLogsQueue.Enqueue(e.Data);

    public void FlushLogs(string name, ConcurrentQueue<string> logsQueue)
    {
        var sb = new StringBuilder();
        while (logsQueue.TryDequeue(out var log))
        {
            sb.AppendLine(log);
        }

        if (sb.Length > 0)
        {
            TestContext.WriteLine($"[{name}]{Environment.NewLine}{sb}");
        }
    }
}
