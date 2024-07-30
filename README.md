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
Once http-server-sim is installed as a global tool, it can be executed from any folder.
For other versions and more information, visit [NuGet](https://www.nuget.org/packages/http-server-sim) and [dotnet tool install](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-tool-install).

### Rule file
The simulation of endpoints is based on a rule file. Here’s how it can be set up:

1. Create a rule file defining the endpoints and their behaviors.
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

Example command (assuming that the file rule.json is in the current folder where the command http-server-sim is being executed):
```bash
http-server-sim --Rules rules.json
```

The following POST request is handled using rule `customers-post`:
```bash
curl --location 'http://localhost:5000/customers' --header 'Content-Type: application/json' --data '{"id":10,"name":"Juan"}' -v
```
`http-server-sim` returns a response with 200. 


The following GET request:
```bash
curl --location 'http://localhost:5000/customers' -v
```

returns 404 with content `Rule matching request not found`

http-server-sim output:
```bash
Request:
Accept: */*
Host: localhost:5000
User-Agent: curl/8.7.1
Protocol: HTTP/1.1
Method: GET
Scheme: http
PathBase:
Path: /customers

warn - HttpServerSim.App
Rule matching request not found.

Response:
StatusCode: 404
```

### http-server-sim CLI options

| Option                           | Description                                                                                       |
|----------------------------------|---------------------------------------------------------------------------------------------------|
| --ControlUrl `<url>`             | URL for managing rules dynamically. Not required. Example: `http://localhost:5001`.               |
| --Help                           | Prints the help.                                                                                  |
| --LogControlRequestAndResponse   | Whether control requests and responses are logged. Default: `false`.                              |
| --LogRequestAndResponse          | Whether requests and responses are logged. Default: `true`.                                       |
| --Rules `<file-name> \| <path>`  | Rules file. It can be a file name of a file that exists in the current directory or a full path to a file. |
| --Url `<url>`                    | URL for simulating endpoints. Default: `http://localhost:5000`                                    |
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
- The request method is POST, and
- The URL path contains /customers.

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

Example of a response defining a message with status code 400.

```json
"response": {
  "statusCode": 400
  }
```

Example of a response defining a message with status code 200 and headers `Server` (with a single value) and `header-1` (with multiple values)

```json
"response": {
  "statusCode": 200,
  "headers": [
    { "key": "Server", "value": ["http-server-sim"] },
    { "key": "header-1", "value": ["val-1", "val-2"] }
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

Example of a response using a json file that exists in the current directory and compressing the content with gzip.

```json
"response": {
  "contentType": "application/json",
  "contentValue": "person-1.json",
  "contentValueType": "File",
  "encoding": "GZip"
}
```