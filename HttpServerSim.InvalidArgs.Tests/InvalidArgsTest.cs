#nullable disable

using HttpServerSim.Tests.Shared;

namespace HttpServerSim.InvalidArgs.Tests;

[TestClass]
public class InvalidArgsTest
{
    private HttpServerSimHostTest _host;

    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void TestInitialize()
    {
        var args = "--CurrentDirectory directory --More more Other --DefaultStatusCode 301 --Logging:LogLevel:HttpServerSim Debug";
        _host = new HttpServerSimHostTest(TestContext, args);    
    }

    [TestMethod]
    public void ArgsValidation_Given_invalid_args_Should_print_message()
    {
        _host.RunAndWaitForExit(TimeSpan.FromSeconds(10));
        
        var actualLogs = _host.FlushLogs();
        var expectedLogs = $"Invalid options:{Environment.NewLine}\tCurrentDirectory{Environment.NewLine}\tMore{Environment.NewLine}\tOther";
 
        Assert.IsTrue(actualLogs.Contains(expectedLogs), "Expected logs not found.");
    }
}