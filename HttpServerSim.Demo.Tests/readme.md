# HttpServerSim.Demo.Tests

This project contains demo tests for `http-server-sim`.

The tests expect a simulator running at `http://localhost:5000` and a control endpoint available at `http://localhost:5001`.

## Run tests against `HttpServerSim.App`

Start `HttpServerSim.App` before running the tests.

One option is to run the project directly by using a launch profile or command-line arguments that include:

```bash
--ControlUrl http://localhost:5001
```

## Run tests against `http-server-sim` installed as a .NET tool

If `http-server-sim` is installed as a global .NET tool, you can start it manually before running the tests.

Run `http-server-sim` from the `./HttpServerSim.App` directory:

```bash
http-server-sim --ControlUrl http://localhost:5001
```
