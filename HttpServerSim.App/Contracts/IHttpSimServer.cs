namespace HttpServerSim.Contracts
{
    public interface IHttpSimServer : IDisposable
    {
        IHttpSimRuleManager CreateRule(string name);
    }
}
