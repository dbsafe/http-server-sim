// Ignore Spelling: json

using System.Reflection;

namespace HttpServerSim.App.RequestResponseLogger.Tests;

[TestClass]
public class FileRequestResponseLoggerPresentationTest
{
    private readonly string _simulatorUrl = AppInitializer.TestHost.SimulatorUrl;
    private static readonly HttpClient _httpClient = AppInitializer.TestHost.HttpClient;
    private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);

    private static readonly string _testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException("Executable not found");
    private static readonly string _historyFolder = Path.Combine(_testDirectory, "http-server-sim-history");

    [TestInitialize]
    public void TestInitialize()
    {
        AppInitializer.TestHost.FlushLogs();

        foreach (var file in GeHistoryFiles())
        {
            File.Delete(file);
        }

        Assert.IsFalse(Directory.EnumerateFiles(_historyFolder!).Any(), $"{nameof(_historyFolder)} should be empty.");
    }

    [TestCleanup]
    public void TestCleanup()
    {
        AppInitializer.TestHost.FlushLogs();
    }

    [TestMethod]
    public async Task Given_a_request_Response_and_request_files_should_be_created()
    {
        await _httpClient.GetAsync($"{_simulatorUrl}/simple-request");

        Assert.IsTrue(WaitForFilesInHistoryDirectory(2), "Expected files are not present in the history folder");

        var expectedRequestContent = @"HTTP/1.1 - GET - http://localhost:5000/simple-request
Headers:
  Host: localhost:5000
Body:
[Not present]";

        AssertFileContent(".req", expectedRequestContent);

        var expectedResponseContent = @"Status Code: 200
Headers:
[Not present]
Body:
[Not present]";

        AssertFileContent(".res", expectedResponseContent);
    }

    private static void AssertFileContent(string ext, string expectedContent)
    {
        var filename = GeHistoryFiles().Where(f => f.EndsWith(ext)).FirstOrDefault();
        Assert.IsNotNull(filename, $"File with extension '{ext}' not found.");
        var actualContent = File.ReadAllText(filename);
        Assert.AreEqual(expectedContent, actualContent, $"Content in file '{filename}' does not match the expected content.");
    }

    private static bool WaitForFilesInHistoryDirectory(int count)
    {
        var expiration = DateTimeOffset.UtcNow + _timeout;
        while (expiration > DateTimeOffset.UtcNow)
        {
            if (GeHistoryFiles().Count() == count)
            {
                return true;
            }

            Thread.Sleep(200);
        }

        return false;
    }

    private static IEnumerable<string> GeHistoryFiles()
    {
        return Directory.EnumerateFiles(_historyFolder!, "*.*").Where(f => f.EndsWith(".res") || f.EndsWith(".req"));
    }
}
