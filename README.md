# http-server-sim

[![Build Status](https://dev.azure.com/dbsafe/dbsafe/_apis/build/status%2Fhttp-server-sim%2Fhttp-server-sim-2024?branchName=main)](https://dev.azure.com/dbsafe/dbsafe/_build/latest?definitionId=13&branchName=main)

HTTP Server Simulator is a .NET tool that runs a Web API simulating HTTP endpoints, supporting the development and testing of components that use HTTP.

http-server-sim can called from the shell/command line.

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
http-server-sim --HttpServerSim:RulesPath rules.json --HttpServerSim:Url http://localhost:5000 --HttpServerSim:LogRequestAndResponse true
```

The following POST request is handled using the rule `customers-post`:
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

### http-server-sim options

| Option                                     | Description                                       | Value/Example           |
|--------------------------------------------|---------------------------------------------------|-------------------------|
| HttpServerSim:RulesPath                    | Rule file with predefined rules                   | `rules.json`            |
| HttpServerSim:Url                          | URL for simulating endpoints                      | `http://localhost:5000` |
| HttpServerSim:LogRequestAndResponse        | Whether requests and responses are logged         | `true`                  |
| HttpServerSim:ControlUrl                   | URL for managing rules dynamically                | `http://localhost:5001` |
| HttpServerSim:LogControlRequestAndResponse | Whether control requests and responses are logged | `true`                  |

## Rule Conditions

When `http-server-sim` processes a request, it uses rule conditions to match a rule to the request. 

Example of conditions:
```json
"conditions": [
  { "field": "Method", "operator": "Equals", "value": "POST" },
  { "field": "Path", "operator": "Contains", "value": "/customers" }
]
```

This condition applies when:
- The request method is POST, and
- The URL path contains /customers.

There are two types of conditions, `Method` and `Path`

1. **Method Conditions**: These specify the HTTP method (e.g., `GET`, `POST`) that the request must match.
2. **URL Path Conditions**: These specify the URL path that the request must match.

The supported operators are: `Equals`, `StartWith`, and `Contains`


