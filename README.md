# http-server-sim

[![Build Status](https://dev.azure.com/dbsafe/dbsafe/_apis/build/status%2Fhttp-server-sim%2Fhttp-server-sim-2024?branchName=main)](https://dev.azure.com/dbsafe/dbsafe/_build/latest?definitionId=13&branchName=main)

HTTP Server Simulator is a .NET tool that runs a Web API simulating HTTP endpoints, supporting the development and testing of components that use HTTP.

http-server-sim can be called from the shell/command line.

## Usage

### Installation
Install the latest version from NuGet:
```bash
dotnet tool install --global http-server-sim
```
Once `http-server-sim` is installed as a global tool, it can run from any folder.
For other versions and more information, visit [NuGet](https://www.nuget.org/packages/http-server-sim) and [dotnet tool install](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-tool-install).

### Running http-server-sim

**Running `http-server-sim` using default options:**
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

`http-server-sim` attempts to match a request to a rule. When a rule is found, it responds with the response defined in that rule. If no matching rule is found, it responds with a configurable `default` response, which has a `Status Code` 200 and no content.

**Running `http-server-sim` setting the default content:**
```bash
# Start the simulator setting the default content with a json
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

Another example setting the default response indicating that a resource not was found.
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
`http-server-sim` returns a response with `Status Code` 200. 

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
| --Url `<url>`                    | URL for simulating endpoints. Default: `http://localhost:5000`.                                   |
|                                  | `--Url` and `--ControlUrl` cannot share the same value.                                           |
||



## Rule conditions

When `http-server-sim` processes a request, it uses rule conditions to match a rule to the request. 

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

When `http-server-sim` identifies a rule that matches a request, it prepares a response message based on the `response` section of the rule.

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