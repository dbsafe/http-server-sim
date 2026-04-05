# http-server-sim

[![Build Status](https://dev.azure.com/dbsafe/dbsafe/_apis/build/status%2Fhttp-server-sim%2Fhttp-server-sim-2024?branchName=main)](https://dev.azure.com/dbsafe/dbsafe/_build/latest?definitionId=13&branchName=main)
[![NuGet version](https://badge.fury.io/nu/http-server-sim.svg)](https://badge.fury.io/nu/http-server-sim)

HTTP Server Simulator is a .NET tool that runs a Web API simulating HTTP endpoints, supporting the development and testing of components that use HTTP.

Use **http-server-sim** to simulate HTTP endpoints during local development, manual testing, and automated test execution.

It is especially useful when you need to:

- Run a lightweight HTTP API locally without building a custom stub service.
- Return predefined responses for specific request patterns.
- Change the simulated server behavior from automated tests.
- Capture incoming requests and responses sent by the simulator into files for later inspection.
- Verify how an application behaves when an endpoint returns specific content, headers, status codes, or delays.

**http-server-sim** supports three common usage modes:

- Manual testing with predefined rules:
  Start **http-server-sim** manually and load a rules file that defines how requests should be handled.

- Automated testing with predefined rules:
  Start **http-server-sim** from the test framework and load a rules file as part of the test setup.

- Automated testing with dynamic rules:
  Control **http-server-sim** from the test itself by creating, updating, and verifying rules at runtime.


## Usage

### Installation
Install the latest version from NuGet:

```bash
dotnet tool install --global http-server-sim
```

Once **http-server-sim** is installed as a global tool, it can be run from any folder.

To install a specific version or learn more about .NET tool installation, see [NuGet package http-server-sim](https://www.nuget.org/packages/http-server-sim) and [dotnet tool install](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-tool-install).

### Running http-server-sim

The quickest way to get started is to run **http-server-sim** with the default options.

By default, the simulator listens on `http://localhost:5000`. When a request matches a rule, the simulator returns the response defined by that rule. When no matching rule is found, it returns the configurable `default` response, which uses status code `200` and no content unless you override it.

**Start the simulator with the default configuration:**
```bash
# Start the simulator
http-server-sim
```

```bash
# Send a GET request to the simulator from another terminal
curl --location 'http://localhost:5000/data' -v
```

Simulator output
```bash
Request:
HTTP/1.1 - GET - http://localhost:5000/data
Headers:
  Accept: */*
  Host: localhost:5000
  User-Agent: curl/8.7.1
Body:
[Not present]
End of Request


Response:
Status Code: 200
Headers:
[Not present]
Body:
[Not present]
End of Response
```

In this example, no rule matches the request, so **http-server-sim** returns the default response: HTTP `200` with no headers and no body.

**Start the simulator with a custom default response body:**
```bash
# Start the simulator and return a JSON body when no rule matches
http-server-sim --DefaultContentType application/json --DefaultContentValue "{""name"":""Juan""}"
```

```bash
# Send a GET request to the simulator from another terminal
curl --location 'http://localhost:5000/data' -v
```

Simulator output
```bash
...
Response:
Status Code: 200
Headers:
  Content-Type: application/json
Body:
{"name":"Juan"}
End of Response
```

In this example, unmatched requests still return status code `200`, but now include a JSON body.

Another example of setting a default response to indicate that a resource was not found:
```bash
http-server-sim --DefaultContentType text/plain --DefaultContentValue "Resource not found" --DefaultStatusCode 404
```

### Rule file
To simulate endpoint behavior, **http-server-sim** can load rules from a JSON rule file.

A rule usually contains:

1. A `name` that identifies the rule.
2. One or more `conditions` used to match incoming requests.
3. A `response` that defines what the simulator should return when the rule matches.

Place the rule file in the current directory or provide a full path to it when starting the simulator.

Example rule file:
```json
{
  "rules": [
    {
      "name": "customers-post",
      "description": "",
      "conditions": [
        { "field": "Method", "operator": "Equals", "value": "POST" },
        { "field": "Path", "operator": "Contains", "value": "/customers" }
      ],
      "response": {
        "statusCode": 200
      }
    }
  ]
}
```

In this example, the rule matches `POST` requests whose path contains `/customers` and returns HTTP `200`.

```bash
# Load rules from a file in the current directory
http-server-sim --Rules rules.json
```

```bash
# Send a POST request that will be handled by the rule named customers-post
curl --location 'http://localhost:5000/customers' --header 'Content-Type: application/json' --data '{"id":10,"name":"Juan"}' -v
```
**http-server-sim** returns a response with status code `200`.

### Saving request and response messages to files

You can save incoming requests and outgoing responses to files for later inspection. This is useful when debugging integration flows or verifying what the simulator received and returned.

```bash
# Save request and response messages to the messages-history directory
http-server-sim --SaveRequests messages-history --SaveResponses messages-history
```

- The directory can be a full path or a relative path under the current directory, for example `messages-history`.
- Messages are saved using a `GUID` as the name, with the `.req` extension for request messages and the `.res` extension for response messages.
- Keep in mind that files are not deleted automatically. If you have a long-running process creating files, the hard drive may eventually run out of space.

### http-server-sim CLI options

The following command-line options are available when starting **http-server-sim**:

| Option                           | Description                                                                                       |
|----------------------------------|---------------------------------------------------------------------------------------------------|
| --ControlUrl `<url>`             | URL for managing rules dynamically. Optional. Example: `http://localhost:5001`.                  |
| --DefaultContentType `<value>`   | Content-Type used in the default response when no rule matches the request.                       |
| --DefaultContentValue `<value>`  | Content used in the default response when no rule matches the request.                            |
| --DefaultDelayMin `<value>`      | Delay in milliseconds before sending the default response when no rule matches the request. Default: `0`. |
| --DefaultDelayMax `<value>`      | Maximum delay in milliseconds before sending the default response when no rule matches the request. |
| --DefaultStatusCode `<value>`    | HTTP status code used in the default response when no rule matches the request. Default: `200`.  |
| --Help                           | Prints the help.                                                                                  |
| --LogControlRequestAndResponse   | Whether control requests and responses are logged. Default: `false`.                              |
| --LogRequestAndResponse          | Whether requests and responses are logged. Default: `true`.                                       |
| --RequestBodyLogLimit `<limit>`  | Maximum request body size to log (in bytes). Default: 4096.                                       |
| --ResponseBodyLogLimit `<limit>` | Maximum response body size to log (in bytes). Default: 4096.                                      |
| --Rules `<file-name> \| <path>`  | Rules file. This can be a file in the current directory or a full path to a file.                |
| --SaveRequests `<directory>`     | The directory where request messages are saved.                                                   |
| --SaveResponses `<directory>`    | The directory where response messages are saved.                                                  |
| --Url `<url>`                    | URL used by the simulator for endpoint requests. Default: `http://localhost:5000`.               |

When `--DefaultDelayMax` is specified, the actual delay is a random value between `--DefaultDelayMin` and `--DefaultDelayMax`.

`--Url` and `--ControlUrl` cannot share the same value.

### Configuring application logs

**http-server-sim** generates logs like any other ASP.NET application. You can control log levels by placing an `appsettings.json` file in the current directory or by passing configuration values on the command line.

Example `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware": "Information",
      "HttpServerSim": "Information"
    }
  }
}
```

If you want to reduce framework noise, set `Microsoft.AspNetCore` to `Warning`.

For example, this avoids informational logs like:
```bash
info: Microsoft.AspNetCore.Hosting.Diagnostics[2]
      Request finished HTTP/1.1 GET http://localhost:5000/customers - 200 - application/json 77.6842ms
```

You can do this either by passing `--Logging:LogLevel:Microsoft.AspNetCore Warning` on the command line or by setting `"Microsoft.AspNetCore": "Warning"` in `appsettings.json`.

## Rule conditions

When **http-server-sim** processes a request, it evaluates the rule conditions to determine whether a rule matches that request.

Example conditions:
```json
"conditions": [
  { "field": "Method", "operator": "Equals", "value": "POST" },
  { "field": "Path", "operator": "Contains", "value": "/customers" }
]
```

A rule with these conditions is applied when:
- The request method is `POST`, **and**
- The URL path contains `/customers`.

The supported condition fields are `Method` and `Path`:

1. `Method`: matches the HTTP method, such as `GET` or `POST`.
2. `Path`: matches the request URL path.

The supported operators are `Equals`, `StartWith`, and `Contains`.


## Rule response messages

When **http-server-sim** finds a matching rule, it builds the response from the rule's `response` section.

### Response properties:

| Property | Description |
|----------|-------------|
| statusCode | Status code of the response. Default: `200`. |
| headers | Collection of response headers. Example: `"headers": [ { "key": "Server", "value": ["http-server-sim"] } ]` |
| contentType | Content type for the response body. This value is used to generate the `Content-Type` header. Example: `application/json` |
| contentValue | Response body content. This can be text, a full path to a file, or a file name when the file exists in the current directory. Example: `person-1.json` |
| contentValueType | Defines whether `contentValue` should be interpreted as text or as a file. Possible values: `Text` or `File`. Default: `Text` |
| encoding | Encoding applied to the response content. Default: `None`. Supported value: `GZip` |

Example of a response that returns status code `400`:

```json
"response": {
  "statusCode": 400
  }
```

Example of a response that returns status code `200` and custom headers:

```json
"response": {
  "statusCode": 200,
  "headers": [
    { "key": "Server", "value": ["http-server-sim"] },
    { "key": "Header-1", "value": ["val-1", "val-2"] }
  ]
}
```

Example of a response with plain text content:

```json
"response": {
  "contentType": "text/plain",
  "contentValue": "Thank you for the notification",
}
```

Example of a response that reads JSON content from a file in the current directory:

```json
"response": {
  "contentType": "application/json",
  "contentValue": "person-1.json",
  "contentValueType": "File"
}
```

Example of a response with inline JSON content:

```json
"response": {
  "contentType": "application/json",
  "contentValue": "{\"name\":\"Juan\"}"
}
```

Example of a response that reads JSON from a file and compresses it with `gzip`:

```json
"response": {
  "contentType": "application/json",
  "contentValue": "person-1.json",
  "contentValueType": "File",
  "encoding": "GZip"
}
```

## Rule delays

When **http-server-sim** identifies a matching rule for a request, it applies any delay configured for that rule before sending the response.

Delays are optional and are expressed in milliseconds. If no delay is configured, the default is `0`.

Delays are useful when testing timeout, retry, and latency-sensitive behavior.

Example of setting a delay of 5 seconds.
```json
"delay": {
  "min": 5000
}
```

Example of setting a random delay between 1 and 10 seconds.
```json
"delay": {
  "min": 1000,
  "max": 10000
}
```

## Using http-server-sim for Test Automation

**http-server-sim** can be used in automated tests either by loading predefined rules or by controlling the simulator dynamically from the test.

To host and control **http-server-sim** from automated tests, use the classes provided by [NuGet package http-server-sim-client](https://www.nuget.org/packages/http-server-sim-client).

For end-to-end examples, see [Examples of using **http-server-sim** for Test Automation](/Users/ernesto/Documents/Repos/valcarcelperez/http-server-sim/Examples/README.MD).

### Hosting http-server-sim in a test

The `HttpServerSimHost` class can be used to start **http-server-sim** as a process inside the test lifecycle.

```cs
// Constructor
public HttpServerSimHost(string simulatorUrl, string workingDirectory, string filenameOrCommand, string args)
```

Constructor parameters:

1. `simulatorUrl`: URL used by the simulator for endpoint requests.
2. `workingDirectory`: directory used by the simulator process.
3. `filenameOrCommand`: executable name or command used to start **http-server-sim**.
4. `args`: command-line arguments passed to the simulator.

Example of starting the simulator during test initialization:
```cs
[TestInitialize]
public void Initialize()
{
    testHost = new HttpServerSimHost(simulatorUrl, testDirectory, "http-server-sim", $"--ControlUrl http://localhost:5001 --DefaultStatusCode 404 --Logging:LogLevel:HttpServerSim Debug --Logging:LogLevel:Microsoft.AspNetCore Warning");
    testHost.Start();
}
```

Example of collecting logs and shutting down the simulator during test cleanup:
```cs
[TestCleanup]
public void Cleanup()
{
    if (testHost is null) return;

    var sb = new StringBuilder();
    while (testHost.LogsQueue.TryDequeue(out var log))
    {
        sb.AppendLine(log);
    }

    TestContext.WriteLine($"[HttpServerSimHost]{Environment.NewLine}{sb}");

    testHost?.Stop();
    testHost?.Dispose();
}
```

### Controlling http-server-sim dynamically

The `HttpSimClient` class is used to communicate with **http-server-sim** through its control endpoint. When the simulator is started with the `--ControlUrl` option, tests can create, update, delete, and verify rules at runtime.

```cs
// Constructor
public HttpSimClient(string controlUrl)
```

Example of creating the client and clearing any existing rules:
```cs
var httpSimClient = new HttpSimClient(controlUrl);
httpSimClient.ClearRules();
```

Rules can be defined using the `RuleBuilder` class.

Example of creating and adding a rule:

```cs
// Define a rule to respond with a specific response message when a request has '/employees' in the path.
var getCustomerRule = RuleBuilder.CreateRule("get-employees")
    .WithCondition(Field.Path, Operator.Contains, "/employees")
    .WithResponse(new HttpSimResponse { StatusCode = 200, ContentType = "application/json", ContentValue = employeesJson })
    .Rule;

// Create a rule dynamically
httpSimClient.AddRule(getCustomerRule);
```

Example of a rule with multiple conditions:

```cs
var conditionMethodEqualsGet = new ConfigCondition { Field = Field.Method, Operator = Operator.Equals, Value = "GET" };
var conditionPathContainsEmployees = new ConfigCondition { Field = Field.Path, Operator = Operator.Contains, Value = "/employees" };

var getCustomerRule = RuleBuilder.CreateRule("get-employees")
    // Setting multiple conditions
    .WithConditions([conditionMethodEqualsGet, conditionPathContainsEmployees])
    .WithResponse(new HttpSimResponse { StatusCode = 200, ContentType = "application/json", ContentValue = employeesJson })
    .Rule;
```

Example of a rule that returns `text/plain` content and uses chained conditions:

```cs
var rule = RuleBuilder.CreateRule("get-employee-info")
    .WithCondition(field: Field.Method, op: Operator.Equals, value: "GET")
    .WithCondition(field: Field.Path, op: Operator.Contains, value: "employees/info/1")
    .WithTextResponse("employee 1 info")
    .Rule;
```

Example of a rule that returns JSON content and response headers:
```cs
var employee = new { Id = 1, Name = "name-1" };
var headers = new KeyValuePair<string, string[]>[]
{
    new("header-1", ["header-11", "header-12"])
};

var rule = RuleBuilder.CreateRule("get-employee")
    .WithCondition(field: Field.Method, op: Operator.Equals, value: "GET")
    .WithJsonResponse(employee, headers)
    .Rule;
```

Example of a rule that returns JSON content compressed with gzip:

```cs
var employee = new { Id = 1, Name = "name-1" };
var headers = new KeyValuePair<string, string[]>[]
{
    new("Content-Encoding", ["gzip"])
};

var rule = RuleBuilder.CreateRule("get-employee-1-compressed")
    .WithCondition(Field.Path, Operator.Contains, "/employees/1")
    .WithJsonResponse(employee, headers, encoding: HttpSimResponseEncoding.GZip)
    .Rule;
```

Example of a rule that returns content from a file:
```cs
var rule = RuleBuilder.CreateRule("get-employee-1-from-file")
    .WithCondition(Field.Path, Operator.Contains, "/employees/1")
    .ReturnResponseFromFile("employee-1.json")
    .Rule;
```

Example of a rule that returns status code `610` with no content:
```cs
var rule = RuleBuilder.CreateRule("get-employees-status-code")
    .WithCondition(field: Field.Method, op: Operator.Equals, value: "GET")
    .ReturnWithStatusCode(610)
    .Rule;
```

The `HttpSimClient` class can be used to verify how rules are applied when handling requests.

Example of verifying that the rule named `get-employee-info` was used twice:
```cs
httpSimClient.VerifyThatRuleWasUsed("get-employee-info", 2);
```

Example of verifying that the last request handled by a specific rule contains the expected JSON content:
```cs
var expected = "{\"id\": 1, \"name\": \"name-1\"}";
httpSimClient.VerifyLastRequestBodyAsJson("create-employee", expected);
```
