﻿namespace HttpServerSim.App.Contracts;

/// <summary>
/// Defines the contract of a rule store in the server
/// </summary>
public interface IHttpSimRuleStore
{
    void CreateRule(IHttpSimRule rule);
    IEnumerable<IHttpSimRule> GetRules();
    void Clear();
    bool DeleteRule(string name);
}
