using HttpServerSim.Models;

namespace HttpServerSim.Contracts;

public interface IHttpSimRuleResolver
{
    HttpSimRule? Resolve(HttpSimRequest request);
}
