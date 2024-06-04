// Ignore Spelling: Json

using FluentAssertions;
using HttpServerSim.Contracts;
using HttpServerSim.Models;
using System.Text;
using System.Text.Json;

namespace HttpServerSim.Services;

public class HttpSimRuleBuilder(string name) : IHttpSimRuleManager
{
    public HttpSimRule Rule { get; } = new HttpSimRule(name);

    public IHttpSimRuleManager Callback(Action<HttpSimRequest> callback)
    {
        if (Rule.Callback != null)
        {
            throw new InvalidOperationException($"{nameof(Callback)} cannot be set more than once");
        }

        Rule.Callback = callback;
        return this;
    }

    public IHttpSimRuleManager ReturnHttpResponse(HttpSimResponse response)
    {
        EnsureResponseIsNotSet();
        Rule.Response = response;
        return this;
    }

    public IHttpSimRuleManager ReturnJson(object content)
    {
        ArgumentNullException.ThrowIfNull(content);
        var jsonContent = JsonSerializer.Serialize(content);
        return LocalReturnText(jsonContent, "application/json");
    }

    public IHttpSimRuleManager ReturnText(string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        return LocalReturnText(content, "text/plain");
    }

    public IHttpSimRuleManager ReturnTextFromFile(string path, string contentType)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(path);
        }

        var httpSimResponse = new HttpSimResponse
        {
            StatusCode = 200,
            ContentValue = path,
            ContentType = contentType,
            ContentValueType = ContentValueType.File
        };

        return ReturnHttpResponse(httpSimResponse);
    }

    public IHttpSimRuleManager ReturnStatusCode(int statusCode)
    {
        return ReturnHttpResponse(new HttpSimResponse { StatusCode = statusCode });
    }

    public IHttpSimRuleManager ReturnFromCallback(Func<HttpSimRequest, HttpSimResponse> createResponseCallback)
    {
        EnsureResponseIsNotSet();
        Rule.CreateResponseCallback = createResponseCallback;
        return this;
    }

    public IHttpSimRuleManager When(Func<HttpSimRequest, bool> ruleEvaluationFunc)
    {
        if (Rule.RuleEvaluationFunc != HttpSimRule.UnspecifiedRuleEvaluationFunc)
        {
            throw new InvalidOperationException($"{nameof(When)} cannot be set more than once");
        }

        Rule.RuleEvaluationFunc = httpSimRequest =>
        {
            var result = ruleEvaluationFunc(httpSimRequest);
            if (result)
            {
                Interlocked.Increment(ref Rule._matchCount);
            }

            return result;
        };

        return this;
    }

    private IHttpSimRuleManager LocalReturnText(string content, string contentType)
    {
        var httpSimResponse = new HttpSimResponse
        {
            StatusCode = 200,
            ContentValue = content,
            ContentType = contentType
        };

        return ReturnHttpResponse(httpSimResponse);
    }

    private void EnsureResponseIsNotSet()
    {
        if (Rule.Response != null || Rule.CreateResponseCallback != null)
        {
            throw new InvalidOperationException("Return cannot be set more than once");
        }
    }

    public IHttpSimRuleManager VerifyThatRuleWasUsed(int times)
    {
        Rule.RuleUsedCount.Should().Be(times);
        return this;
    }
}
