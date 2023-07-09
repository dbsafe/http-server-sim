// Ignore Spelling: Json Deserialized Api

#nullable disable

using FluentAssertions;
using HttpServerSim.Models;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace HttpServerSim.Demo.Tests;

[TestClass]
public class HttpServerSimTest
{
    private static readonly HttpClient _client = new();
    private static readonly object _syncObj = new();
    private ApiHttpSimServer _apiHttpSimServer;
    private static readonly string _testFilesLocation = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestFiles");

    private readonly HttpServerConfig _httpServerConfig = new("http://localhost:8081", args: []);
    // Separate url to target a running server instead of server running with the test
    private readonly string _integrationUrl = "http://localhost:8080";

    [TestInitialize]
    public void Initialize()
    {
        Monitor.Enter(_syncObj);
        _apiHttpSimServer = new ApiHttpSimServer(_httpServerConfig);
        _apiHttpSimServer.Start();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _apiHttpSimServer?.Stop();
        _apiHttpSimServer?.Dispose();
        Monitor.Exit(_syncObj);
    }

    #region CreateRule Method

    [TestMethod]
    public void CreateRule_ShouldReturnNotNull()
    {
        var actual = _apiHttpSimServer.CreateRule("get-employees");

        actual.Should().NotBeNull();
    }

    [TestMethod]
    public void CreateRule_GivenARuleThatAlreadyExists_ShouldThrow()
    {
        _apiHttpSimServer.CreateRule("get-employees")
            .When(request => request.Method == "GET")
            .ReturnJson(200);

        Action act = () =>
        {
            _apiHttpSimServer.CreateRule("get-employees")
            .When(request => request.Method == "POST")
            .ReturnJson(200);
        };

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("A rule with the same name already exists");
    }

    #endregion

    #region When Method

    [TestMethod]
    public void When_ShouldReturnNotNull()
    {
        var actual = _apiHttpSimServer.CreateRule("get-employees")
            .When(request => request.Method == "GET");

        actual.Should().NotBeNull();
    }

    [TestMethod]
    public void When_GivenWhenIsSet_ShouldThrow()
    {
        var ruleBuilder = _apiHttpSimServer.CreateRule("get-employees")
            .When(request => request.Method == "GET");

        Action act = () => ruleBuilder.When(request => request.Method == "POST");
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("When cannot be set more than once");
    }

    #endregion

    #region ReturnHttpResponse Method

    [TestMethod]
    public Task ReturnHttpResponse_ShouldReturnTheResponse()
    {
        return ReturnHttpResponse_ShouldReturnTheResponse_Common(_httpServerConfig.Url);
    }

    [TestMethod]
    [TestCategory("Integration")]
    public Task ReturnHttpResponse_ShouldReturnTheResponse_Integration()
    {
        return ReturnHttpResponse_ShouldReturnTheResponse_Common(_integrationUrl);
    }

    private async Task ReturnHttpResponse_ShouldReturnTheResponse_Common(string url)
    {
        var headers = new KeyValuePair<string, string[]>[]
        {
            new("header-1", ["header-11", "header-12"])
        };

        var contentJson = JsonSerializer.Serialize(new { Id = 1, Name = "name-1" });
        var apiResponse = new HttpSimResponse { StatusCode = 200, ContentValue = contentJson, ContentType = "application/json", Headers = headers };

        _apiHttpSimServer.CreateRule("get-employees")
            .When(request => request.Method == "GET")
            .ReturnHttpResponse(apiResponse);

        var actualHttpResponse = await _client.GetAsync($"{url}/employees");

        AssertResponse(apiResponse, actualHttpResponse);
    }

    #endregion

    #region ReturnJson Method

    [TestMethod]
    public Task ReturnJson_ShouldReturnAResponseWithAJsonContent()
    {
        return ReturnJson_ShouldReturnAResponseWithAJsonContent_Common(_httpServerConfig.Url);
    }

    [TestMethod]
    [TestCategory("Integration")]
    public Task ReturnJson_ShouldReturnAResponseWithAJsonContent_Integration()
    {
        return ReturnJson_ShouldReturnAResponseWithAJsonContent_Common(_integrationUrl);
    }

    private async Task ReturnJson_ShouldReturnAResponseWithAJsonContent_Common(string url)
    {
        var headers = new KeyValuePair<string, string[]>[]
        {
            new("header-1", ["header-11", "header-12"])
        };

        var apiJsonResponseContent = new { Id = 1, Name = "name-1" };

        _apiHttpSimServer.CreateRule("get-employees")
            .When(request => request.Method == "GET")
            .ReturnJson(apiJsonResponseContent);

        var actualHttpResponse = await _client.GetAsync($"{url}/employees");

        AssertJsonResponse(apiJsonResponseContent, actualHttpResponse);
    }

    #endregion

    #region ReturnText Method

    [TestMethod]
    public Task ReturnText_ShouldReturnTheResponseWithATextContent()
    {
        return ReturnText_ShouldReturnTheResponseWithATextContent_Common(_httpServerConfig.Url);
    }

    [TestMethod]
    [TestCategory("Integration")]
    public Task ReturnText_ShouldReturnTheResponseWithATextContent_Integration()
    {
        return ReturnText_ShouldReturnTheResponseWithATextContent_Common(_integrationUrl);
    }

    private async Task ReturnText_ShouldReturnTheResponseWithATextContent_Common(string url)
    {
        _apiHttpSimServer.CreateRule("get-employees")
            .When(request => request.Method == "GET")
            .ReturnText("employee info");

        var actualHttpResponse = await _client.GetAsync($"{url}/employees-as-text");

        AssertTextResponse("employee info", actualHttpResponse);
    }

    #endregion

    #region ReturnTextFromFile Method

    [TestMethod]
    public Task ReturnTextFromFile_ShouldReturnTheResponseWithATextFromAFile()
    {
        return ReturnTextFromFile_ShouldReturnTheResponseWithATextFromAFile_Common(_httpServerConfig.Url);
    }

    [TestMethod]
    [TestCategory("Integration")]
    public Task ReturnTextFromFile_ShouldReturnTheResponseWithATextFromAFile_Integration()
    {
        return ReturnTextFromFile_ShouldReturnTheResponseWithATextFromAFile_Common(_integrationUrl);
    }

    private async Task ReturnTextFromFile_ShouldReturnTheResponseWithATextFromAFile_Common(string url)
    {
        var path = Path.Combine(_testFilesLocation, "employee-1.json");

        _apiHttpSimServer.CreateRule("get-employee-from-file")
            .When(request => request.Method == "GET")
            .ReturnTextFromFile(path, "application/json");

        var actualHttpResponse = await _client.GetAsync($"{url}/employee-from-file");

        AssertContentType("application/json", actualHttpResponse.Content.Headers);

        var expectedContent = File.ReadAllText(path);
        AssertTextContent(expectedContent, actualHttpResponse.Content);
    }

    #endregion

    #region ReturnStatusCode Method

    [TestMethod]
    public Task ReturnStatusCode_ShouldReturnAResponseWithTheStatusCode()
    {
        return ReturnStatusCode_ShouldReturnAResponseWithTheStatusCode_Common(_httpServerConfig.Url);
    }

    [TestMethod]
    [TestCategory("Integration")]
    public Task ReturnStatusCode_ShouldReturnAResponseWithTheStatusCode_Integration()
    {
        return ReturnStatusCode_ShouldReturnAResponseWithTheStatusCode_Common(_integrationUrl);
    }

    private async Task ReturnStatusCode_ShouldReturnAResponseWithTheStatusCode_Common(string url)
    {
        _apiHttpSimServer.CreateRule("get-employees-status-code")
            .When(request => request.Method == "GET")
            .ReturnStatusCode(404);

        var actualHttpResponse = await _client.GetAsync($"{url}/employees-status-code");

        actualHttpResponse.Should().NotBeNull();

        AssertStatusCode(404, actualHttpResponse.StatusCode);
        AssertContent(expectedContent: null, expectedContentType: null, actualHttpResponse.Content);
    }

    #endregion

    [TestMethod]
    public async Task ReturnFromCallback_ShouldReturnTheResponseThatWasSetInTheCallbackFunction()
    {
        _apiHttpSimServer.CreateRule("get-employees")
            .When(request => request.Method == "GET")
            .ReturnFromCallback(httpSimRequest => new HttpSimResponse { StatusCode = 401 });

        var actualHttpResponse = await _client.GetAsync($"{_httpServerConfig.Url}/a-path");

        actualHttpResponse.Should().NotBeNull();

        AssertStatusCode(401, actualHttpResponse.StatusCode);
        AssertContent(expectedContent: null, expectedContentType: null, actualHttpResponse.Content);
    }

    [TestMethod]
    public void ReturnStatusCode_GivenReturnIsSet_ShouldThrow()
    {
        var ruleBuilder = _apiHttpSimServer.CreateRule("get-employees")
            .When(request => request.Method == "GET")
            .ReturnText("some-text");

        Action act = () =>
        {
            ruleBuilder.ReturnStatusCode(404);
        };

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Return cannot be set more than once");
    }

    #region Processing HTTP Requests

    [TestMethod]
    public async Task GivenARequest_ShouldReturnTheResponseFromAMatchingRule()
    {
        _apiHttpSimServer.CreateRule("get")
            .When(request => request.Method == "GET")
            .ReturnStatusCode(200);

        _apiHttpSimServer.CreateRule("delete")
            .When(request => request.Method == "DELETE")
            .ReturnStatusCode(401);

        var getHttpResponse = await _client.GetAsync($"{_httpServerConfig.Url}/employees");
        ((int)getHttpResponse.StatusCode).Should().Be(200);

        var deleteHttpResponse = await _client.DeleteAsync($"{_httpServerConfig.Url}/employees");
        ((int)deleteHttpResponse.StatusCode).Should().Be(401);
    }

    #endregion

    #region VerifyThatRuleWasUsed Method

    [TestMethod]
    public async Task VerifyThatRuleWasUsed_ShouldNotThrowWhenCorrect()
    {
        var getRule = _apiHttpSimServer.CreateRule("get")
            .When(request => request.Method == "GET")
            .ReturnStatusCode(200);

        var deleteRule = _apiHttpSimServer.CreateRule("delete")
            .When(request => request.Method == "DELETE")
            .ReturnStatusCode(401);

        var updateRule = _apiHttpSimServer.CreateRule("update")
            .When(request => request.Method == "UPDATE")
            .ReturnStatusCode(200);

        await _client.GetAsync($"{_httpServerConfig.Url}/employees");
        await _client.DeleteAsync($"{_httpServerConfig.Url}/employees");
        await _client.GetAsync($"{_httpServerConfig.Url}/employees");

        getRule.VerifyThatRuleWasUsed(2);
        deleteRule.VerifyThatRuleWasUsed(1);
        updateRule.VerifyThatRuleWasUsed(0);
    }

    [TestMethod]
    public void VerifyThatRuleWasUsed_ShouldThrowWhenIncorrect()
    {
        var getRule = _apiHttpSimServer.CreateRule("get")
            .When(request => request.Method == "GET")
            .ReturnStatusCode(200);

        Action act = () => getRule.VerifyThatRuleWasUsed(2);

        act.Should()
            .Throw<AssertFailedException>()
            .WithMessage("Expected Rule.RuleUsedCount to be 2, but found 0 (difference of -2).");
    }

    #endregion

    #region Callback Method

    [TestMethod]
    public async Task GivenRuleWithCallback_ShouldExecuteCallback()
    {
        long callBackCount = 0;

        var getListRule = _apiHttpSimServer.CreateRule("create-employee")
            .When(request => request.Method == "POST" && request.Path.Contains("/employees"))
            .ReturnJson(new Employee { Id = 3, FirstName = "fn-3", LastName = "ln-3" })
            .Callback(simRequest => Interlocked.Increment(ref callBackCount));

        var target = CreateEmployeesApiClient();

        await target.CreateAsync(new Employee { FirstName = "fn-3", LastName = "ln-3" });
        Interlocked.Read(ref callBackCount).Should().Be(1);

        await target.CreateAsync(new Employee { FirstName = "fn-4", LastName = "ln-4" });
        Interlocked.Read(ref callBackCount).Should().Be(2);
    }

    [TestMethod]
    public async Task GivenARequestWithAPayload_HttpSimRequestGetContentObject_ShouldReturnTheDeserializedContent()
    {
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        Employee actualContentAsObject = default;
        var getListRule = _apiHttpSimServer.CreateRule("create-employee")
            .When(request => request.Method == "POST" && request.Path.Contains("/employees"))
            .ReturnStatusCode(200)
            .Callback(httpSimRequest => actualContentAsObject = httpSimRequest.GetContentObject<Employee>(jsonSerializerOptions));

        var employee = new Employee
        {
            Id = 10,
            FirstName = "fn-10",
            LastName = "ln-10"
        };

        var actualHttpResponse = await _client.PostAsJsonAsync($"{_httpServerConfig.Url}/employees", employee);

        actualContentAsObject.Should().BeEquivalentTo(employee);
    }

    #endregion

    private EmployeesApiClient CreateEmployeesApiClient() => new(_httpServerConfig.Url);

    private static void AssertResponse(HttpSimResponse expected, HttpResponseMessage actual)
    {
        actual.Should().NotBeNull();

        AssertStatusCode(expected.StatusCode, actual.StatusCode);
        AssertHeaders(expected.Headers, actual.Headers);
        AssertContent(expected.ContentValue, expected.ContentType, actual.Content);
    }

    private static void AssertJsonResponse(object expectedContent, HttpResponseMessage actual)
    {
        actual.Should().NotBeNull();

        AssertStatusCode(200, actual.StatusCode);
        AssertContentType("application/json", actual.Content.Headers);
        AssertJsonContent(expectedContent, actual.Content);
    }

    private static void AssertTextResponse(string expectedContent, HttpResponseMessage actual)
    {
        actual.Should().NotBeNull();

        AssertStatusCode(200, actual.StatusCode);
        AssertContentType("text/plain", actual.Content.Headers);
        AssertTextContent(expectedContent, actual.Content);
    }

    private static void AssertHeaders(KeyValuePair<string, string[]>[] expectedHeaders, HttpResponseHeaders actualHeaders)
    {
        if (expectedHeaders == null)
        {
            return;
        }

        actualHeaders.Should().NotBeNull();
        foreach (var expectedKvpHeader in expectedHeaders)
        {
            var actualKvpHeader = actualHeaders.FirstOrDefault(h => h.Key == expectedKvpHeader.Key);
            actualKvpHeader.Should().NotBeNull();

            foreach (var expectedHeader in expectedKvpHeader.Value)
            {
                actualKvpHeader.Value.Should().Contain(expectedHeader);
            }
        }
    }

    private static void AssertJsonContent(object expectedContent, HttpContent actualContent)
    {
        var actualContentBytes = actualContent.ReadAsByteArrayAsync().Result;
        var actualContentString = Encoding.UTF8.GetString(actualContentBytes);
        var actualObject = JsonSerializer.Deserialize<object>(actualContentString);

        var expectedJsonElement = JsonSerializer.SerializeToElement(expectedContent);

        actualObject.Should().NotBeNull();
        actualObject!.ToString().Should().BeEquivalentTo(expectedJsonElement.ToString());
    }

    private static void AssertTextContent(string expectedContent, HttpContent actualContent)
    {
        var actualContentBytes = actualContent.ReadAsByteArrayAsync().Result;
        var actualContentString = Encoding.UTF8.GetString(actualContentBytes);

        actualContentString.Should().BeEquivalentTo(expectedContent);
    }

    private static void AssertContent(string expectedContent, string expectedContentType, HttpContent actualContent)
    {
        if (expectedContent == null)
        {
            actualContent.Headers.ContentType.Should().BeNull();
            return;
        }

        var actualContentAsString = actualContent.ReadAsStringAsync().Result;
        actualContentAsString.Should().BeEquivalentTo(expectedContent);

        AssertContentType(expectedContentType, actualContent.Headers);
    }

    private static void AssertContentType(string expectedContentType, HttpContentHeaders actualContentHeaders)
    {
        if (expectedContentType is null)
        {
            return;
        }

        var actualContentTypeHeaders = actualContentHeaders.TryGetValues("Content-Type", out IEnumerable<string> values) ? values : null;
        actualContentTypeHeaders.Should().Contain(expectedContentType);
    }

    private static void AssertStatusCode(int expectedStatusCode, HttpStatusCode actualStatusCode)
    {
        ((int)actualStatusCode).Should().Be(expectedStatusCode);
    }
}

public interface IEmployeesApiClient
{
    Task<IEnumerable<Employee>> GetListAsync();
    Task<Employee> GetAsync(int id);
    Employee Update(Employee employee);
    Employee Delete(int id);
    Task<Employee> CreateAsync(Employee newEmployee);
}

public class EmployeesApiClient(string url) : IEmployeesApiClient
{
    private static readonly HttpClient _client = new();
    private readonly string _url = url;

    public async Task<Employee> CreateAsync(Employee newEmployee)
    {
        var httpResponse = await _client.PostAsJsonAsync($"{_url}/employees", newEmployee);
        return await ReadFromJsonOrThrowAsync<Employee>(httpResponse);
    }

    private static async Task<T> ReadFromJsonOrThrowAsync<T>(HttpResponseMessage httpResponse)
    {
        if (httpResponse!.Content != null)
        {
            var actualvalue = await httpResponse.Content.ReadFromJsonAsync<T>();

            if (actualvalue != null)
            {
                return actualvalue;
            }
        }

        throw new Exception("Unexpected response");
    }

    public Employee Delete(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Employee> GetAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Employee>> GetListAsync()
    {
        var responseBody = await _client.GetStringAsync($"{_url}/employees");
        return JsonSerializer.Deserialize<IEnumerable<Employee>>(responseBody)!;
    }

    public Employee Update(Employee employee)
    {
        throw new NotImplementedException();
    }
}

public class Employee
{
    public int? Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}