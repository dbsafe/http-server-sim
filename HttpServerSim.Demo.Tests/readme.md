# HttpServerSim.Demo.Tests

Tests for http-server-sim

`http-server-sim` must be running.

### Testing against project HttpServerSim.App
`HttpServerSim.App` must be running before the tests.
Run `HttpServerSim.App` by using a launch profile with command line argument `--ControlUrl http://localhost:5001`


### Testing against http-server-sim installed as a dotnet tool
`http-server-sim` must be already installed.
Run `http-server-sim` in `\HttpServerSim.App`
```bash
http-server-sim --ControlUrl http://localhost:5001
```