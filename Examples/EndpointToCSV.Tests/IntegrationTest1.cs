// Ignore Spelling: json

# nullable disable

using HttpServerSim.Client;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

namespace EndpointToCSV.Tests;

[TestClass]
public class IntegrationTest1
{
    private static readonly string testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new Exception("Null is not expected here.");
    private static readonly string expectedPathToCSVFile = Path.Combine(testDirectory, "customers.csv");

    private readonly ConcurrentQueue<string> endpointToCSVLogsQueue = new();

    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void Initialize()
    {
        if (File.Exists(expectedPathToCSVFile))
        {
            File.Delete(expectedPathToCSVFile);
        }
    }

    [TestCleanup]
    public void Cleanup()
    {
        FlushLogs("EndpointToCSV", endpointToCSVLogsQueue);
    }

    [TestMethod]
    public void Given_a_json_response_Should_create_a_csv_file()
    {
        // At this point http-server-sim must be running manually using the command:
        // http-server-sim --Rules rules.json --DefaultStatusCode 404 --Logging:LogLevel:HttpServerSim Debug --Logging:LogLevel:Microsoft.AspNetCore Warning
        // From directory EndpointToCSV.Tests

        using var consoleAppRunner = new ConsoleAppRunner(testDirectory, "dotnet", "EndpointToCSV.dll http://localhost:5000/customers customers.csv");
        consoleAppRunner.OutputDataReceived += ConsoleAppRunner_OutputDataReceived;
        consoleAppRunner.ErrorDataReceived += ConsoleAppRunner_OutputDataReceived;
        var completed = consoleAppRunner.RunAndWaitForExit(TimeSpan.FromSeconds(10));
        Assert.IsTrue(completed, "Process did not complete on time.");

        // At this point EndpointToCSV completed executing:
        // http-server-sim's output should contain the request and response.
        // File customers.csv in the test directory should contain the correct CVS lines.

        Assert.IsTrue(File.Exists(expectedPathToCSVFile), $"Expected output file '{expectedPathToCSVFile}' not found");

        var actualCSVFileContent = File.ReadAllText(expectedPathToCSVFile);
        TestContext.WriteLine($"Content of file: {expectedPathToCSVFile}{Environment.NewLine}{actualCSVFileContent}");
        TestContext.WriteLine("");

        var expectedCSVFileContent = @"Id,FirstName,LastName,Company,City,Country,Phone1,Phone2,Email,SubscriptionDate,Website
DD37Cf93aecA6Dc,Sheryl,Baxter,Rasmussen Group,East Leonard,Chile,229-077-5154,397-884-0519x718,zunigavanessa@smith.info,8/24/2020 12:00:00 AM,http://www.stephenson.com
1Ef7b82A4CAAD10,Preston,Lozano,Vega-Gentry,East Jimmychester,Djibouti,515-343-5776,686-620-1820x944,vmata@colon.com,4/23/2021 12:00:00 AM,http://www.hobbs.com
6F94879bDAfE5a6,Roy,Berry,Murillo-Perry,Isabelborough,Antigua and Barbuda,539-402-0259,496-978-3969x58947,beckycarr@hogan.com,3/25/2020 12:00:00 AM,http://www.lawrence.com";

        Assert.AreEqual(expectedCSVFileContent, actualCSVFileContent);
    }

    private void ConsoleAppRunner_OutputDataReceived(object sender, ConsoleAppRunnerEventArgs e)
    {
        endpointToCSVLogsQueue.Enqueue(e.Data);
    }

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
