using HttpServerSim.Models;

namespace HttpServerSim.Contracts;

public interface IHttpSimRuleManager
{
    HttpSimRule Rule { get; }

    IHttpSimRuleManager When(Func<HttpSimRequest, bool> ruleEvaluationFunc);

    IHttpSimRuleManager ReturnHttpResponse(HttpSimResponse response);
    IHttpSimRuleManager ReturnJson(object content);
    IHttpSimRuleManager ReturnText(string content);
    IHttpSimRuleManager ReturnTextFromFile(string path, string contentType);
    IHttpSimRuleManager ReturnStatusCode(int statusCode);
    IHttpSimRuleManager ReturnFromCallback(Func<HttpSimRequest, HttpSimResponse> createResponseCallback);

    IHttpSimRuleManager Callback(Action<HttpSimRequest> callback);

    IHttpSimRuleManager VerifyThatRuleWasUsed(int times);
}
