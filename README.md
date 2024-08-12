# http-server-sim

[![Build Status](https://dev.azure.com/dbsafe/dbsafe/_apis/build/status%2Fhttp-server-sim%2Fhttp-server-sim-2024?branchName=main)](https://dev.azure.com/dbsafe/dbsafe/_build/latest?definitionId=13&branchName=main)

HTTP Server Simulator is a .NET tool that runs a Web API simulating HTTP endpoints, supporting the development and testing of components that use HTTP.

**http-server-sim** can be used for various types of tests:

- Manual Tests Using Predefined Rules:
In this mode, **http-server-sim** is run manually loading a predefined rules file that specifies the behavior for responding to requests.

- Automated Tests Using Predefined Rules:
**http-server-sim** is run by the test framework, loading a predefined rules file that dictates the behavior when responding to requests.

- Automated Tests Using Dynamic Rules:
In this scenario, **http-server-sim** is controlled dynamically by the test. The test can adjust **http-server-sim**'s behavior by managing the rules in real-time. Additionally, the test can request parameters from **http-server-sim** and use them to verify the behavior of the target application.


## Usage

### Installation
Install the latest version from NuGet:
```bash
dotnet tool install --global http-server-sim
```
Once **http-server-sim** is installed as a global tool, it can run from any folder.
For other versions and more information, visit [NuGet package http-server-sim](https://www.nuget.org/packages/http-server-sim) and [dotnet tool install](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-tool-install).

### Running http-server-sim

**Running http-server-sim using default options:**
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

**http-server-sim** attempts to match a request to a rule. When a rule is found, it responds with the response defined in that rule. If no matching rule is found, it responds with a configurable `default` response, which has a `Status Code` 200 and no content.

**Running http-server-sim setting a default response content:**
```bash
# Start the simulator setting a default content with a json
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

Another example of setting a default response indicating that a resource not was found.
```bash
http-server-sim --DefaultContentType text/plain --DefaultContentValue "Resource not found" --DefaultStatusCode 404
```

### Rule file
The simulation of endpoints is based on a rule file. Here’s how it can be set up:

1. Create a rule file defining conditions and a response message.
2. Place the rule file in the appropriate directory.
3. Run the simulator with the rule file.

Example of rule file.
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

```bash
# Loading rules from a file in the current directory
http-server-sim --Rules rules.json
```

```bash
# Send a POST request (will be handled by the rule called customers-post):
curl --location 'http://localhost:5000/customers' --header 'Content-Type: application/json' --data '{"id":10,"name":"Juan"}' -v
```
**http-server-sim** returns a response with `Status Code` 200. 

### Saving request and response messages to files

```bash
# Save request and response to the folder messages-history under the current directory
http-server-sim --SaveRequests messages-history --SaveResponses messages-history
```

- The directory can be a full path, e.g., `C:\temp\http-server-sim-messages`, or a relative directory under the current directory, e.g., `messages-history`.
- Messages are saved using a `GUID` as the name, with the `.req` extension for request messages and the `.res` extension for response messages.
- Keep in mind that files are not deleted automatically. If you have a long-running process creating files, the hard drive may eventually run out of space.

### http-server-sim CLI options

| Option                           | Description                                                                                       |
|----------------------------------|---------------------------------------------------------------------------------------------------|
| --ControlUrl `<url>`             | URL for managing rules dynamically. Not required. Example: `http://localhost:5001`.               |
| --DefaultContentType `<value>`   | The Content-Type used in a response message when no rule matching the request is found.           |
| --DefaultContentValue `<value>`  | The Content used in a response message when no rule matching the request is found.                |
| --DefaultStatusCode `<value>`    | The HTTP status code used in a response message when no rule matching the request is found. Default: 200.|
| --Help                           | Prints the help.                                                                                  |
| --LogControlRequestAndResponse   | Whether control requests and responses are logged. Default: `false`.                              |
| --LogRequestAndResponse          | Whether requests and responses are logged. Default: `true`.                                       |
| --RequestBodyLogLimit `<limit>`  | Maximum request body size to log (in bytes). Default: 4096.                                       |
| --ResponseBodyLogLimit `<limit>` | Maximum response body size to log (in bytes). Default: 4096.                                      |
| --Rules `<file-name> \| <path>`  | Rules file. It can be a file name of a file that exists in the current directory or a full path to a file. |
| --SaveRequests `<directory>`     | The directory where request messages are saved.                                                   |
| --SaveResponses `<directory>`    | The directory where response messages are saved.                                                  |
| --Url `<url>`                    | URL for simulating endpoints. Default: `http://localhost:5000`.                                   |
|                                  | `--Url` and `--ControlUrl` cannot share the same value.                                           |
||

### Configuring application logs

**http-server-sim** generates logs like any other ASP.NET application. Log levels can be controlled by using an `appsettings.json` file in the current directory or by passing optional parameters.

Configuration file
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

To avoid information logs from `Microsoft.AspNetCore` like this one
```bash
info: Microsoft.AspNetCore.Hosting.Diagnostics[2]
      Request finished HTTP/1.1 GET http://localhost:5000/customers - 200 - application/json 77.6842ms
```

add option `--Logging:LogLevel:Microsoft.AspNetCore Warning` to the command line or use an `appsettings.json` file setting `"Microsoft.AspNetCore": "Warning"`

## Rule conditions

When **http-server-sim** processes a request, it uses rule conditions to match a rule to the request. 

Example of conditions:
```json
"conditions": [
  { "field": "Method", "operator": "Equals", "value": "POST" },
  { "field": "Path", "operator": "Contains", "value": "/customers" }
]
```

A rule with these conditions is applied when:
- The request method is `POST`, **and**
- The URL path contains `/customers`.

There are two types of conditions, `Method` and `Path`

1. **Method Conditions**: These specify the HTTP method (e.g., `GET`, `POST`) that the request must match.
2. **URL Path Conditions**: These specify the URL path that the request must match.

The supported operators are: `Equals`, `StartWith`, and `Contains`


## Response messages

When **http-server-sim** identifies a rule that matches a request, it prepares a response message based on the `response` section of the rule.

### Response properties:

| Property | Description |
|----------|-------------|
| statusCode | Status code of the response message. Default: 200 |
| headers | A collection of headers. Example: `"headers": [ { "key": "Server", "value": ["http-server-sim"] } ]` |
| contentType | Content type of a response with a content, this value is used to create the header `Content-Type`. Example: `application/json` |
| contentValue | Value used in the content, can be a text, a full path to a file, or a file name when the file exists in the current directory. Example: `person-1.json` |
| contentValueType | Defines whether `contentValue` is a text or a file. Possible values: `Text` or `File`. Default: `Text`|
| encoding | Defines an encoding to apply to the content. Default: `None`. Supported values: `GZip`
||

Example of a response defining a message with `Status Code` 400.

```json
"response": {
  "statusCode": 400
  }
```

Example of a response defining a message with `Status Code` 200 and headers `Server` (with a single value) and `Header-1` (with multiple values)

```json
"response": {
  "statusCode": 200,
  "headers": [
    { "key": "Server", "value": ["http-server-sim"] },
    { "key": "Header-1", "value": ["val-1", "val-2"] }
  ]
}
```

Example of a response with a plain text content.

```json
"response": {
  "contentType": "text/plain",
  "contentValue": "Thank you for the notification",
}
```

Example of a response using a json file that exists in the current directory.

```json
"response": {
  "contentType": "application/json",
  "contentValue": "person-1.json",
  "contentValueType": "File"
}
```

Example of a response using an in-line json.

```json
"response": {
  "contentType": "application/json",
  "contentValue": "{\"name\":\"Juan\"}"
}
```

Example of a response using a json file that exists in the current directory and compressing the content with `gzip`.

```json
"response": {
  "contentType": "application/json",
  "contentValue": "person-1.json",
  "contentValueType": "File",
  "encoding": "GZip"
}
```

## Using http-server-sim for Test Automation

**http-server-sim** can be used to implement test automation. Its behavior can be controlled dynamically from the test by creating rules on the fly.

In order to host and control **http-server-sim** you need to used classes defined in [NuGet package http-server-sim-client](https://www.nuget.org/packages/http-server-sim-client)

[Examples of using **http-server-sim** for Test Automation](/Examples/README.MD)

### Hosting http-server-sim in a test

The `HttpServerSimHost` class can be used to run **http-server-sim** as a process within the test.

```cs
// Constructor
public HttpServerSimHost(string simulatorUrl, string workingDirectory, string filenameOrCommand, string args)
```
```cs
// Initializing and starting http-server-sim
[TestInitialize]
public void Initialize()
{
    testHost = new HttpServerSimHost(simulatorUrl, testDirectory, "http-server-sim", $"--ControlUrl http://localhost:5001 --DefaultStatusCode 404 --Logging:LogLevel:HttpServerSim Debug --Logging:LogLevel:Microsoft.AspNetCore Warning");
    testHost.Start();
}
```
```cs
// Printing logs from http-server-sim
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

### Controlling http-sever-sim's behavior dynamically

The `HttpSimClient` class can be used to communicate with **http-server-sim**. When **http-server-sim** is run with the `--ControlUrl` option, it listens on a control endpoint that is accessed by `HttpSimClient`.

```cs
// Constructor
public HttpSimClient(string controlUrl)
```

```cs
// Instantiating HttpSimClient and deleting existent rules
var httpSimClient = new HttpSimClient(controlUrl);
httpSimClient.ClearRules();
```

Rules can be defined using the `RuleBuilder` class.

Example of creating and adding a rule.

```cs
// Define a rule to respond with a specific response message when a request has '/employees' in the path.
var getCustomerRule = RuleBuilder.CreateRule("get-employees")
    .WithCondition(Field.Path, Operator.Contains, "/employees")
    .WithResponse(new HttpSimResponse { StatusCode = 200, ContentType = "application/json", ContentValue = employeesJson })
    .Rule;

// Create a rule dynamically
httpSimClient.AddRule(getCustomerRule);
```

Example of a rule that uses multiple conditions.

```cs
var conditionMethodEqualsGet = new ConfigCondition { Field = Field.Method, Operator = Operator.Equals, Value = "GET" };
var conditionPathContainsEmployees = new ConfigCondition { Field = Field.Path, Operator = Operator.Contains, Value = "/employees" };

var getCustomerRule = RuleBuilder.CreateRule("get-employees")
    // Setting multiple conditions
    .WithConditions([conditionMethodEqualsGet, conditionPathContainsEmployees])
    .WithResponse(new HttpSimResponse { StatusCode = 200, ContentType = "application/json", ContentValue = employeesJson })
    .Rule;
```

Example of a rule that returns a `text/plain` content. Demonstrates a second way of setting multiple conditions.

```cs
var rule = RuleBuilder.CreateRule("get-employee-info")
    .WithCondition(field: Field.Method, op: Operator.Equals, value: "GET")
    .WithCondition(field: Field.Path, op: Operator.Contains, value: "employees/info/1")
    .WithTextResponse("employee 1 info")
    .Rule;
```

Example of a rule that returns a message with JSON content and response headers.
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

Example of a rule that returns JSON content compressed with gzip.

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

Example of a rule that returns content from a file.
```cs
var rule = RuleBuilder.CreateRule("get-employee-1-from-file")
    .WithCondition(Field.Path, Operator.Contains, "/employees/1")
    .ReturnResponseFromFile("employee-1.json")
    .Rule;
```

Example of a rule that returns a Status Code 610 with no content.
```cs
var rule = RuleBuilder.CreateRule("get-employees-status-code")
    .WithCondition(field: Field.Method, op: Operator.Equals, value: "GET")
    .ReturnWithStatusCode(610)
    .Rule;
```

The `HttpSimClient` class can be used to verify how rules are applied when handling requests.

Example of verifying that the rule named get-employee-info was used twice.
```cs
httpSimClient.VerifyThatRuleWasUsed("get-employee-info", 2);
```

Example of verifying that the last request handled by a specific rule contains the expected JSON content.
```cs
var expected = "{\"id\": 1, \"name\": \"name-1\"}";
httpSimClient.VerifyLastRequestBodyAsJson("create-employee", expected);
```