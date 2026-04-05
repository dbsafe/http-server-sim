# Examples of Using http-server-sim for Test Automation

This folder contains a small sample application and a companion test project that demonstrate different ways of using **http-server-sim** in automated tests.

## Projects

- `EndpointToCSV`: a console application that reads customer data from a URL and writes it to a CSV file.
- `EndpointToCSV.Tests`: integration tests for `EndpointToCSV` that simulate the upstream HTTP endpoint with **http-server-sim**.

The sample is intentionally simple. In a real application, the same testing patterns can be used around more complex data transformations and domain logic.

## Configuring `EndpointToCSV.Tests`

To allow the tests to run the sample application:

- `EndpointToCSV` is referenced by `EndpointToCSV.Tests` as a project dependency.
- The output of `EndpointToCSV` is copied to the test output directory so the tests can execute `EndpointToCSV.dll`.

## Test examples

All three integration tests validate the same result: data returned by the HTTP endpoint is used to create the expected CSV file. The difference is how **http-server-sim** is started and configured.

### `IntegrationTest1`

This test demonstrates the most manual workflow. `EndpointToCSV` is executed by the test, but **http-server-sim** must already be running outside the test process.

Before running the test:

- Start **http-server-sim** manually.
- Run it from the `./EndpointToCSV.Tests` directory.
- Use `rules.json`, which defines a rule that returns the JSON content stored in `customers.json`.

Start **http-server-sim** with:

```bash
http-server-sim --Rules rules.json --DefaultStatusCode 404 --Logging:LogLevel:HttpServerSim Debug --Logging:LogLevel:Microsoft.AspNetCore Warning
```

What this test demonstrates:

- Using a predefined rule file.
- Manually inspecting the simulator output.
- Verifying that the application creates the expected `customers.csv` file.

### `IntegrationTest2`

This test automates the simulator startup. `EndpointToCSV` is still exercised as an external process, but **http-server-sim** is started and stopped by the test itself.

The simulator is started with:

```bash
--Rules rules.json --DefaultStatusCode 404 --Logging:LogLevel:HttpServerSim Debug --Logging:LogLevel:Microsoft.AspNetCore Warning
```

Prerequisites:

- `rules.json` defines a rule that returns the JSON content from `customers.json`.
- `rules.json` and `customers.json` must be copied to the test output directory as part of the build.

What this test demonstrates:

- Hosting **http-server-sim** from the test by using `HttpServerSimHost`.
- Reusing a predefined rule file without requiring any manual setup.
- Collecting simulator logs as part of the test output.

### `IntegrationTest3`

This test demonstrates dynamic rule configuration. **http-server-sim** is started by the test, but instead of loading rules from a file, the test creates rules at runtime through the control endpoint.

The simulator is started with:

```bash
--ControlUrl http://localhost:5001 --DefaultStatusCode 404 --Logging:LogLevel:HttpServerSim Debug --Logging:LogLevel:Microsoft.AspNetCore Warning
```

What this test demonstrates:

- Starting **http-server-sim** with a control endpoint.
- Creating rules dynamically with `HttpSimClient` and `RuleBuilder`.
- Verifying that a rule was used after the application makes the request.
- Validating the generated CSV output from a dynamically defined response.
