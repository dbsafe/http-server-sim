// Ignore Spelling: Json Deserialized Api

#nullable disable

using FluentAssertions;
using HttpServerSim.Client;
using HttpServerSim.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace HttpServerSim.Demo.Tests;

[TestClass]
public class HttpSimClientTest
{
    private static readonly HttpClient _httpClient = HttpClientFactory.CreateHttpClient(nameof(HttpSimClientTest));
    private static readonly object _syncObj = new();
    private HttpSimClient _httpSimClient;
    private static readonly string _testFilesLocation = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestFiles");

    private readonly string _simUrl = "http://localhost:8080";
    private readonly string _controlUrl = "http://localhost:8090";

    [TestInitialize]
    public void Initialize()
    {
        Monitor.Enter(_syncObj);
        _httpSimClient = new HttpSimClient(_controlUrl);
        _httpSimClient.ClearRules();
    }

    [TestCleanup]
    public void Cleanup()
    {
        Monitor.Exit(_syncObj);
    }

    [TestMethod]
    public async Task WithResponse()
    {
        var headers = new KeyValuePair<string, string[]>[]
        {
            new("header-1", ["header-11", "header-12"])
        };

        var employee = new { Id = 1, Name = "name-1" };
        var contentJson = JsonSerializer.Serialize(employee);
        var apiResponse = new HttpSimResponse { StatusCode = 200, ContentValue = contentJson, ContentType = "application/json", Headers = headers };

        var rule = RuleBuilder.CreateRule("create-employee")
            .WithCondition(field: Field.Method, op: Operator.Equals, value: "POST")
            .WithResponse(apiResponse)
            .Rule;
        _httpSimClient.AddRule(rule);

        var actualHttpResponse = await _httpClient.PostAsJsonAsync($"{_simUrl}/employees", employee);

        AssertResponse(apiResponse, actualHttpResponse);
    }

    [TestMethod]
    public async Task WithJsonResponse()
    {
        var employee = new { Id = 1, Name = "name-1" };
        var headers = new KeyValuePair<string, string[]>[]
        {
            new("header-1", ["header-11", "header-12"])
        };

        var rule = RuleBuilder.CreateRule("create-employee")
            .WithCondition(field: Field.Method, op: Operator.Equals, value: "POST")
            .WithJsonResponse(employee, headers: headers)
            .Rule;
        _httpSimClient.AddRule(rule);

        var actualHttpResponse = await _httpClient.PostAsJsonAsync($"{_simUrl}/employees", employee);

        AssertJsonResponse(employee, actualHttpResponse);
        AssertHeaders(headers, actualHttpResponse.Headers);
    }

    [TestMethod]
    public async Task WithTextResponse_With_Multiple_Conditions()
    {
        var rule = RuleBuilder.CreateRule("get-employees")
            .WithCondition(field: Field.Method, op: Operator.Equals, value: "GET")
            .WithCondition(field: Field.Path, op: Operator.Contains, value: "employees-as-text")
            .WithTextResponse("employee info")
            .Rule;
        _httpSimClient.AddRule(rule);

        var actualHttpResponse = await _httpClient.GetAsync($"{_simUrl}/employees-as-text");

        AssertTextResponse("employee info", actualHttpResponse);
    }

    [TestMethod]
    public async Task ReturnTextResponseFromFile()
    {
        var path = Path.Combine(_testFilesLocation, "employee-1.json");

        var rule = RuleBuilder.CreateRule("get-employees")
            .WithCondition(field: Field.Path, op: Operator.Contains, value: "employee-from-file")
            .ReturnTextResponseFromFile(path)
            .Rule;
        _httpSimClient.AddRule(rule);

        var actualHttpResponse = await _httpClient.GetAsync($"{_simUrl}/employee-from-file");

        AssertContentType("application/json", actualHttpResponse.Content.Headers);

        var expectedContent = File.ReadAllText(path);
        AssertTextContent(expectedContent, actualHttpResponse.Content);
    }

    [TestMethod]
    public async Task ReturnWithStatusCode()
    {
        var rule = RuleBuilder.CreateRule("get-employees-status-code")
            .WithCondition(field: Field.Method, op: Operator.Equals, value: "GET")
            .ReturnWithStatusCode(610)
            .Rule;
        _httpSimClient.AddRule(rule);

        var actualHttpResponse = await _httpClient.GetAsync($"{_simUrl}/employees-status-code");

        actualHttpResponse.Should().NotBeNull();

        AssertStatusCode(610, actualHttpResponse.StatusCode);
        AssertContent(expectedContent: null, expectedContentType: null, actualHttpResponse.Content);
    }

    [TestMethod]
    public async Task GivenARequest_ShouldReturnTheResponseFromAMatchingRule()
    {
        var employee1 = new { Id = 1, Name = "employee-1" };
        var employee2 = new { Id = 2, Name = "employee-2" };
        var employees = new object[] { employee1, employee2 };
        var rule1 = RuleBuilder.CreateRule("get")
            .WithCondition(field: Field.Method, op: Operator.Equals, value: "GET")
            .WithJsonResponse(employees)
            .Rule;

        var rule2 = RuleBuilder.CreateRule("delete")
            .WithCondition(field: Field.Method, op: Operator.Equals, value: "DELETE")
            .ReturnWithStatusCode(401)
        .Rule;

        _httpSimClient.AddRules([rule1, rule2]);

        var getHttpResponse = await _httpClient.GetAsync($"{_simUrl}/employees");
        ((int)getHttpResponse.StatusCode).Should().Be(200);

        var deleteHttpResponse = await _httpClient.DeleteAsync($"{_simUrl}/employees");
        ((int)deleteHttpResponse.StatusCode).Should().Be(401);
    }

    [TestMethod]
    public async Task VerifyThatRuleWasUsed_ShouldNotThrowWhenCorrect()
    {
        var getRule = RuleBuilder.CreateRule("get")
            .WithCondition(field: Field.Method, op: Operator.Equals, value: "GET")
            .ReturnWithStatusCode(200)
            .Rule;

        var deleteRule = RuleBuilder.CreateRule("delete")
            .WithCondition(field: Field.Method, op: Operator.Equals, value: "DELETE")
            .ReturnWithStatusCode(401)
            .Rule;

        var updateRule = RuleBuilder.CreateRule("update")
            .WithCondition(field: Field.Method, op: Operator.Equals, value: "UPDATE")
            .ReturnWithStatusCode(200)
            .Rule;

        _httpSimClient.AddRules([getRule, deleteRule, updateRule]);

        await _httpClient.GetAsync($"{_simUrl}/employees");
        await _httpClient.DeleteAsync($"{_simUrl}/employees");
        await _httpClient.GetAsync($"{_simUrl}/employees");

        _httpSimClient.VerifyThatRuleWasUsed(getRule.Name, 2);
        _httpSimClient.VerifyThatRuleWasUsed(deleteRule.Name, 1);
        _httpSimClient.VerifyThatRuleWasUsed(updateRule.Name, 0);
    }

    [TestMethod]
    public void VerifyThatRuleWasUsed_ShouldThrowWhenIncorrect()
    {
        var getRule = RuleBuilder.CreateRule("get")
            .WithCondition(field: Field.Method, op: Operator.Equals, value: "GET")
            .ReturnWithStatusCode(200)
            .Rule;
        _httpSimClient.AddRule(getRule);

        Action act = () => _httpSimClient.VerifyThatRuleWasUsed(getRule.Name, 2);

        act.Should()
            .Throw<AssertFailedException>()
            .WithMessage("Expected ruleHits to be 2, but found 0 (difference of -2).");
    }

    [TestMethod]
    public async Task VerifyLastRequestBodyAsJson_ShouldNotThrowWhenCorrect()
    {
        var postRule = RuleBuilder.CreateRule("post")
            .WithCondition(field: Field.Method, op: Operator.Equals, value: "POST")
            .ReturnWithStatusCode(200)
            .Rule;
        _httpSimClient.AddRule(postRule);

        await _httpClient.PostAsJsonAsync($"{_simUrl}/employees", new { id = 1, name = "name-1" });

        var expected = @"{
    ""id"": 1,
    ""name"": ""name-1""
}";
        
        _httpSimClient.VerifyLastRequestBodyAsJson(postRule.Name, expected);
    }

    [TestMethod]
    public async Task VerifyLastRequestBodyAsJson_ShouldThrowWhenIncorrect()
    {
        var postRule = RuleBuilder.CreateRule("post")
            .WithCondition(field: Field.Method, op: Operator.Equals, value: "POST")
            .ReturnWithStatusCode(200)
            .Rule;
        _httpSimClient.AddRule(postRule);

        await _httpClient.PostAsJsonAsync($"{_simUrl}/employees", new { id = 1 });

        var expected = @"{
    ""id"": 2
}";

        try
        {
            _httpSimClient.VerifyLastRequestBodyAsJson(postRule.Name, expected);
            Assert.Fail("An exception was not thrown");
        }
        catch (AssertFailedException ex)
        {
            Assert.AreEqual("Expected actualJson to be \"{\r\n  \"id\": 2\r\n}\", but \"{\r\n  \"id\": 1\r\n}\" differs near \"1\r\n\" (index 11).", ex.Message);
        }
    }

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
