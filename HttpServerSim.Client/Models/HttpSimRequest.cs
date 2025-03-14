﻿// Ignore Spelling: Json Serializer

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace HttpServerSim.Client.Models;

/// <summary>
/// Defines a HTTP request
/// </summary>
/// <param name="method"></param>
/// <param name="path"></param>
public class HttpSimRequest(string method, string path)
{
    private string? _jsonContent;
    private bool _jsonContentResolved;
    private readonly object _lock = new();

    public string? ContentValue { get; set; }
    public KeyValuePair<string, string[]>[]? Headers { get; set; }
    public string Method { get; } = method;
    public string Path { get; } = path;

    [JsonIgnore]
    public string? JsonContent
    {
        get
        {
            lock (_lock)
            {
                if (!_jsonContentResolved)
                {
                    _jsonContentResolved = true;

                    var contentValue = ContentValue;
                    if (contentValue is null)
                    {
                        return null;
                    }

                    if (TryParse(contentValue, out JsonObject? jsonObject))
                    {
                        _jsonContent = jsonObject?.ToString();
                    }
                }
            }

            return _jsonContent;
        }

        set
        {
            lock (_lock)
            {
                _jsonContentResolved = true;
                _jsonContent = value;
            }
        }
    }

    public TContent? GetContentObject<TContent>(JsonSerializerOptions jsonSerializerOptions)
    {
        var json = JsonContent;
        if (json == null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<TContent>(json, jsonSerializerOptions);
    }

    private static bool TryParse(string json, out JsonObject? jsonObject)
    {
        try
        {
            jsonObject = JsonNode.Parse(json)!.AsObject();
            return true;
        }
        catch
        {
            jsonObject = default;
            return false;
        }
    }
}
