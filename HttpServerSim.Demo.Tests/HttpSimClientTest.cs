// Ignore Spelling: Json Deserialized Api

#nullable disable

using FluentAssertions;
using HttpServerSim.Client;
using HttpServerSim.Client.Models;
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
    private static readonly HttpClient _httpClientResponseCompressed = HttpClientFactory.CreateHttpClient($"{nameof(HttpSimClientTest)}_Encoded", DecompressionMethods.GZip);
    private static readonly object _syncObj = new();
    private HttpSimClient _httpSimClient;
    private static readonly string _testFilesLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    private readonly string _simUrl = "http://localhost:5000";
    private readonly string _controlUrl = "http://localhost:5001";

    private static readonly ConfigRule rule1 = new() { Name = "rule-1", Response = new HttpSimResponse() };
    private static readonly ConfigRule rule2 = new() { Name = "rule-2", Response = new HttpSimResponse() };

    // HttpClient automatically fills in the Host header using the request URI
    // https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-sni#host-header
    private static readonly KeyValuePair<string, string[]> _hostHeader = new("Host", ["localhost:5000"]);

    private static readonly string employee1Json = "{\"id\":1,\"name\":\"name-1\"}";
    private static readonly KeyValuePair<string, string[]> _employee1JsonLengthHeader = new("Content-Length", [employee1Json.Length.ToString()]);

    private static readonly string _contentTypeJson = "application/json; charset=utf-8";
    private static readonly KeyValuePair<string, string[]> _contentTypeJsonHeader = new("Content-Type", [_contentTypeJson]);

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
    public async Task WithResponse_GivenOneResponse_ResponseShouldBeUsedMultipleTimes()
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
        _httpSimClient.GetRuleHits("create-employee").Should().Be(0);

        var actualHttpResponse = await _httpClient.PostAsJsonAsync($"{_simUrl}/employees", employee);
        _httpSimClient.GetRuleHits("create-employee").Should().Be(1);
        AssertResponse(apiResponse, actualHttpResponse);

        actualHttpResponse = await _httpClient.PostAsJsonAsync($"{_simUrl}/employees", employee);
        _httpSimClient.GetRuleHits("create-employee").Should().Be(2);
        AssertResponse(apiResponse, actualHttpResponse);
    }

    [TestMethod]
    public async Task WithResponse_GivenTowResponses_ResponsesShouldBeUsedAlternating()
    {
        var headers = new KeyValuePair<string, string[]>[]
        {
            new("header-1", ["header-11", "header-12"])
        };

        var employee1 = new { Id = 1, Name = "name-1" };
        var contentJson1 = JsonSerializer.Serialize(employee1);
        var apiResponse1 = new HttpSimResponse { StatusCode = 200, ContentValue = contentJson1, ContentType = "application/json", Headers = headers };

        var employee2 = new { Id = 2, Name = "name-2" };
        var contentJson2 = JsonSerializer.Serialize(employee2);
        var apiResponse2 = new HttpSimResponse { StatusCode = 200, ContentValue = contentJson2, ContentType = "application/json", Headers = headers };

        var rule = RuleBuilder.CreateRule("create-employee")
            .WithCondition(field: Field.Method, op: Operator.Equals, value: "POST")
            .WithResponse(apiResponse1)
            .WithResponse(apiResponse2)
            .Rule;
        _httpSimClient.AddRule(rule);
        _httpSimClient.GetRuleHits("create-employee").Should().Be(0);

        var actualHttpResponse = await _httpClient.PostAsJsonAsync($"{_simUrl}/employees", employee1);
        _httpSimClient.GetRuleHits("create-employee").Should().Be(1);
        AssertResponse(apiResponse1, actualHttpResponse);

        actualHttpResponse = await _httpClient.PostAsJsonAsync($"{_simUrl}/employees", employee2);
        _httpSimClient.GetRuleHits("create-employee").Should().Be(2);
        AssertResponse(apiResponse2, actualHttpResponse);

        actualHttpResponse = await _httpClient.PostAsJsonAsync($"{_simUrl}/employees", employee1);
        _httpSimClient.GetRuleHits("create-employee").Should().Be(3);
        AssertResponse(apiResponse1, actualHttpResponse);

        actualHttpResponse = await _httpClient.PostAsJsonAsync($"{_simUrl}/employees", employee2);
        _httpSimClient.GetRuleHits("create-employee").Should().Be(4);
        AssertResponse(apiResponse2, actualHttpResponse);
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
        AssertResponseHeaders(headers, actualHttpResponse.Headers);
    }

    [TestMethod]
    public async Task WithGZipJsonResponse()
    {
        var employee = new { Id = 1, Name = "name-1" };
        var headers = new KeyValuePair<string, string[]>[]
        {
            new("Content-Encoding", ["gzip"])
        };

        var rule = RuleBuilder.CreateRule("create-employee-compressed-response")
            .WithCondition(field: Field.Method, op: Operator.Equals, value: "POST")
            .WithJsonResponse(employee, headers: headers, encoding: HttpSimResponseEncoding.GZip)
            .Rule;
        _httpSimClient.AddRule(rule);

        var actualHttpResponse = await _httpClientResponseCompressed.PostAsJsonAsync($"{_simUrl}/employees", employee);

        AssertJsonResponse(employee, actualHttpResponse);
        actualHttpResponse.Content.GetType().Name.Should().Be("GZipDecompressedContent");
    }

    [TestMethod]
    public async Task WithTextResponse_WithMultipleConditions()
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
        // This file must exists in the test project and in the target project.
        // Both projects are sharing the same file as a link
        var filename = "employee-1.json";

        var rule = RuleBuilder.CreateRule("get-employees")
            .WithCondition(field: Field.Path, op: Operator.Contains, value: "employee-from-file")
            .ReturnResponseFromFile(filename)
            .Rule;
        _httpSimClient.AddRule(rule);

        var actualHttpResponse = await _httpClient.GetAsync($"{_simUrl}/employee-from-file");

        AssertContentType("application/json", actualHttpResponse.Content.Headers);

        var path = Path.Combine(_testFilesLocation, filename);
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
            .WithCondition(field: Field.Method, op: Operator.Equals, value: "PATCH")
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

        var expected = "{\"id\": 1, \"name\": \"name-1\"}";

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
            Assert.IsTrue(ex.Message.StartsWith("Expected actualJson"));
        }
    }

    [TestMethod]
    public async Task GetRequestsByRule_ShouldReturnRequestsThatMatchARule()
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
            .WithCondition(field: Field.Method, op: Operator.Equals, value: "PATCH")
            .ReturnWithStatusCode(200)
            .Rule;

        _httpSimClient.AddRules([getRule, deleteRule, updateRule]);

        await _httpClient.GetAsync($"{_simUrl}/employees");
        await _httpClient.PatchAsJsonAsync($"{_simUrl}/employees", new { id = 1, name = "name-1" });
        await _httpClient.GetAsync($"{_simUrl}/employees");

        IEnumerable<HttpSimRequest> expectedGetRequests =
        [
            new HttpSimRequest("GET", "/employees") { Headers = [ _hostHeader ] },
            new HttpSimRequest("GET", "/employees") { Headers = [ _hostHeader ] },
        ];
        var actualGetRequests = _httpSimClient.GetRequestsByRule("get");
        actualGetRequests.Should().BeEquivalentTo(expectedGetRequests);

        IEnumerable<HttpSimRequest> expectedUpdateRequests =
        [
            new HttpSimRequest("PATCH", "/employees")
            {
                ContentValue = employee1Json,
                Headers = [ _hostHeader, _employee1JsonLengthHeader, _contentTypeJsonHeader]
            }
        ];
        var actualUpdateRequets = _httpSimClient.GetRequestsByRule("update");
        actualUpdateRequets.Should().BeEquivalentTo(expectedUpdateRequests, options => options.Excluding(a => a.JsonContent));
    }

    [TestMethod]
    public void Delete_GivenRuleExists_ShouldDeleteRuleAndReturnTrue()
    {
        CreateRules1And2();

        _httpSimClient.DeleteRule("rule-1").Should().BeTrue();

        _httpSimClient.RuleExists("rule-1").Should().BeFalse();
        _httpSimClient.RuleExists("rule-2").Should().BeTrue();
    }

    [TestMethod]
    public void Delete_GivenRuleDoesNotExist_ShouldReturnFalse()
    {
        CreateRules1And2();

        _httpSimClient.DeleteRule("rule-3").Should().BeFalse();

        _httpSimClient.RuleExists("rule-1").Should().BeTrue();
        _httpSimClient.RuleExists("rule-2").Should().BeTrue();
    }

    [TestMethod]
    public void GetRule_GivenRuleDoesNotExist_ShouldReturnNull()
    {
        CreateRules1And2();

        _httpSimClient.GetRule("rule-3").Should().BeNull();
    }

    [TestMethod]
    public void GetRule_GivenRuleExist_ShouldReturnTheRule()
    {
        CreateRules1And2();

        _httpSimClient.GetRule("rule-1").Should().BeEquivalentTo(rule1);
    }

    [TestMethod]
    public void UpdateRule_GivenRuleExist_ShouldUpdateTheRule()
    {
        CreateRules1And2();

        var rule11 = new ConfigRule() { Name = "rule-1", Response = new HttpSimResponse { StatusCode = 404 } };

        _httpSimClient.UpdateRule(rule11);
        _httpSimClient.GetRule("rule-1").Should().BeEquivalentTo(rule11);
    }

    [TestMethod]
    public void UpdateRule_GivenRuleDoesNotExist_ShouldThrowAnException()
    {
        var action = () =>
        {
            var rule3 = new ConfigRule() { Name = "rule-3", Response = new HttpSimResponse() };
            _httpSimClient.UpdateRule(rule3);
        };

        action.Should().Throw<Exception>().WithMessage("Rule 'rule-3' not found.");
    }

    private void CreateRules1And2()
    {
        _httpSimClient.AddRules([rule1, rule2]);

        _httpSimClient.RuleExists(rule1.Name).Should().BeTrue();
        _httpSimClient.RuleExists(rule2.Name).Should().BeTrue();
    }

    private static void AssertResponse(HttpSimResponse expected, HttpResponseMessage actual)
    {
        actual.Should().NotBeNull();

        AssertStatusCode(expected.StatusCode, actual.StatusCode);
        AssertResponseHeaders(expected.Headers, actual.Headers);
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

    private static void AssertResponseHeaders(KeyValuePair<string, string[]>[] expectedHeaders, HttpResponseHeaders actualHeaders)
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

    private static void AssertContentHeaders(KeyValuePair<string, string[]>[] expectedHeaders, HttpContentHeaders actualHeaders)
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
