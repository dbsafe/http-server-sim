﻿// Ignore Spelling: Json Serializer

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HttpServerSim.Models;

public class HttpSimRequest(string method, string path) : HttpSimMessage
{
    private string? _jsonContent;
    private bool _jsonContentResolved;
    private readonly object _lock = new();

    public string Method { get; } = method;
    public string Path { get; } = path;

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
