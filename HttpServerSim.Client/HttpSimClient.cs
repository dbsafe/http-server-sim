// Ignore Spelling: Json

using FluentAssertions;
using HttpServerSim.Client.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServerSim.Client;

public class HttpSimClient(string controlUrl)
{
    private static readonly HttpClient _httpClient = HttpClientFactory.CreateHttpClient(nameof(HttpSimClient));
    private readonly string _controlUrl = controlUrl;
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public void ClearRules()
    {
        var response = _httpClient.DeleteAsync($"{_controlUrl}{Routes.RULES}").Result;
        response.EnsureSuccessStatusCode();
        EnsureOperationResultSuccess<OperationResult>(response);
    }

    public void AddRules(ConfigRule[] rules)
    {
        var response = _httpClient.PostAsJsonAsync($"{_controlUrl}{Routes.RULES}", rules, _serializerOptions).Result;
        response.EnsureSuccessStatusCode();
        EnsureOperationResultSuccess<OperationResult>(response);
    }

    public void AddRule(ConfigRule rule)
    {
        AddRules([rule]);
    }

    public ConfigRule[] GetAllRules()
    {
        var response = _httpClient.GetAsync($"{_controlUrl}{Routes.RULES}").Result;
        response.EnsureSuccessStatusCode();
        var operationResult = EnsureOperationResultSuccess<OperationResult<ConfigRule[]>>(response);
        operationResult.Data.Should().NotBeNull($"Rules returned are null");
        return operationResult.Data!;
    }

    public bool RuleExists(string name) => GetAllRules().Any(r => r.Name == name);

    public bool DeleteRule(string name)
    {
        var response = _httpClient.DeleteAsync($"{_controlUrl}{Routes.RuleByName(name)}").Result;
        response.EnsureSuccessStatusCode();

        var operationResult = GetOperationResult<OperationResult>(response);
        return operationResult.Success;
    }

    public ConfigRule? GetRule(string name)
    {
        var response = _httpClient.GetAsync($"{_controlUrl}{Routes.RuleByName(name)}").Result;
        response.EnsureSuccessStatusCode();
        var operationResult = GetOperationResult<OperationResult<ConfigRule>>(response);
        return operationResult.Data;
    }

    public void UpdateRule(ConfigRule rule)
    {
        var content = new StringContent(JsonSerializer.Serialize(rule), Encoding.UTF8, "application/json");
        var response = _httpClient.PutAsync($"{_controlUrl}{Routes.RuleByName(rule.Name)}", content).Result;
        response.EnsureSuccessStatusCode();
        var operationResult = EnsureOperationResultSuccess<OperationResult>(response);
        operationResult.Success.Should().BeTrue();
    }

    public void VerifyThatRuleWasUsed(string name, int times)
    {
        var ruleHits = GetRuleHits(name);
        ruleHits.Should().Be(times);
    }

    public void VerifyLastRequestBodyAsJson(string ruleName, string expectedBody)
    {
        var path = $"{Routes.RULES}/{ruleName}{Routes.REQUESTS}";
        var response = _httpClient.GetAsync($"{_controlUrl}{path}").Result;
        response.EnsureSuccessStatusCode();
        var operationResult = EnsureOperationResultSuccess<OperationResult<IEnumerable<HttpSimRequest>>>(response);
        var lastRequest = operationResult.Data.LastOrDefault();
        lastRequest.Should().NotBeNull("Request not found.");

        lastRequest.JsonContent.Should().NotBeNullOrEmpty("Request don't have a content.");

        var step = "parsingActual";
        try
        {
            using var actual = JsonDocument.Parse(lastRequest.JsonContent!);
            step = "parsingExpected";
            using var expected = JsonDocument.Parse(expectedBody);
            step = "comparing";

            var actualJson = actual.ToJsonString();
            var expectedJson = expected.ToJsonString();
            actualJson.Should().Be(expectedJson);
        }
        catch (Exception ex) when (step == "parsingActual")
        {
            throw new Exception($"Failed to parse actual body.{Environment.NewLine}{lastRequest.JsonContent}", ex);
        }
        catch (Exception ex) when (step == "parsingExpected")
        {
            throw new Exception($"Failed to parse expected body.{Environment.NewLine}{expectedBody}", ex);
        }
        catch
        {
            throw;
        }
    }

    private int GetRuleHits(string ruleName)
    {
        var path = $"{Routes.RULES}/{ruleName}{Routes.HITS}";
        var response = _httpClient.GetAsync($"{_controlUrl}{path}").Result;
        response.EnsureSuccessStatusCode();
        var operationResult = EnsureOperationResultSuccess<OperationResult<int>>(response);
        return operationResult.Data;
    }

    private static TBody EnsureOperationResultSuccess<TBody>(HttpResponseMessage response)
        where TBody : OperationResult
    {
        var operationResult = DeserializeBody<TBody>(response);
        if (!operationResult.Success)
        {
            throw new Exception(operationResult.Message);
        }

        return operationResult;
    }

    private static TBody GetOperationResult<TBody>(HttpResponseMessage response)
        where TBody : OperationResult => DeserializeBody<TBody>(response);

    private static TBody DeserializeBody<TBody>(HttpResponseMessage response)
    {
        var json = response.Content.ReadAsStringAsync().Result;
        var body = JsonSerializer.Deserialize<TBody>(json, _serializerOptions);
        return body is null ? throw new Exception($"Failed to deserialize response.{Environment.NewLine}{json}") : body;
    }
}

internal static partial class JsonDocumentExtensions
{
    public static string ToJsonString(this JsonDocument jsonDoc)
    {
        using var stream = new MemoryStream();
        var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
        jsonDoc.WriteTo(writer);
        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }
}

internal static partial class HttpClientJsonExtensions
{
    public static Task<HttpResponseMessage> PostAsJsonAsync<TValue>(this HttpClient client, string? requestUri, TValue value, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (client is null)
        {
            throw new ArgumentNullException(nameof(client));
        }

        var valueAsString = JsonSerializer.Serialize(value, options);
        var content = new StringContent(valueAsString);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return client.PostAsync(requestUri, content, cancellationToken);
    }
}
