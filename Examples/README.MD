# Examples of using http-server-sim for Test Automation

Projects:
- `EndpointToCSV`: A console application that reads data from a `URL` and generates a `CSV` file. 
  This is a simple example without any transformation of the data. In a real world process there may be data transformations and domain logic that this test would verify.
- `EndpointToCSV.Tests`: Demonstrates how to test `EndpointToCSV` simulating an endpoint with **http-server-sim**.

### Configuring EndpointToCSV.Tests to run EndpointToCSV
- `EndpointToCSV` was added to `EndpointToCSV.Tests` as a project reference.
- In the dependency, property `Copy Local` was set to `Yes` in order to copy the console application `EndpointToCSV` to the output folder with `EndpointToCSV.Tests.dll`.

### Test examples
All the tests aim to validate that the data returned by the endpoint is correctly used to create the output file.

**IntegrationTest1**</p>
Executes and tests `EndpointToCSV` application, **http-server-sim** must be started manually. 

Preconditions:
- **http-server-sim** must be running and using a rules file to respond to requests.
- `rules.json` defines a rule to return the `json` content from file `customers.json`.

Start **http-server-sim** in the directory `./EndpointToCSV.Tests`
```batch
http-server-sim --Rules rules.json --DefaultStatusCode 404 --Logging:LogLevel:HttpServerSim Debug --Logging:LogLevel:Microsoft.AspNetCore Warning
```

Visually confirm that **http-server-sim**'s output includes both the request and the response.

**IntegrationTest2**</p>
Executes and tests the `EndpointToCSV` application, **http-server-sim** is started by the test with the following options:
```bash
--Rules rules.json --DefaultStatusCode 404 --Logging:LogLevel:HttpServerSim Debug --Logging:LogLevel:Microsoft.AspNetCore Warning
```

Preconditions:
- `rules.json` defines a rule to return the `json` content from file `customers.json`. `rules.json` and `customers.json` must be set to be copied to the output directory when building the project.
</p>

**IntegrationTest3**</p>
Executes and tests the `EndpointToCSV` application, **http-server-sim** is started by the test with the following options:
```bash
--ControlUrl http://localhost:5001 --DefaultStatusCode 404 --Logging:LogLevel:HttpServerSim Debug --Logging:LogLevel:Microsoft.AspNetCore Warning
```
Instead of defining rules and responses in a rule file, this test uses `ControlUrl` endpoint for setting rules dynamically and verifying that their were used.
