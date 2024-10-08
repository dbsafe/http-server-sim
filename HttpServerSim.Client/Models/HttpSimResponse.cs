﻿using System.Text.Json.Serialization;

namespace HttpServerSim.Client.Models;

/// <summary>
/// Defines a response HTTP message
/// </summary>
public class HttpSimResponse : HttpSimMessage
{
    public int StatusCode { get; set; } = 200;
    public ContentValueType ContentValueType { get; set; }
    public HttpSimResponseEncoding Encoding { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContentValueType
{
    Text,
    File
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HttpSimResponseEncoding
{
    None,
    GZip
}
