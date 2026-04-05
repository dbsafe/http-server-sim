# Development

This document describes common development tasks for working on `http-server-sim`.

## Running tests locally

### Run the app locally for manual testing

Start `HttpServerSim.App` without the debugger, then run the tests or exercise the simulator manually.

### Run the automated test projects

The repository contains multiple test projects. You can run them individually with `dotnet test`.

Examples:

```bash
dotnet test ./HttpServerSim.App.Tests/HttpServerSim.App.Tests.csproj
dotnet test ./HttpServerSim.App.RequestResponseLogger.Tests/HttpServerSim.App.RequestResponseLogger.Tests.csproj
dotnet test ./HttpServerSim.App.Rules.Tests/HttpServerSim.App.Rules.Tests.csproj
dotnet test ./HttpServerSim.App.StaticRules.Tests/HttpServerSim.App.StaticRules.Tests.csproj
dotnet test ./HttpServerSim.App.CommandLineArgs.Tests/HttpServerSim.App.CommandLineArgs.Tests.csproj
dotnet test ./HttpServerSim.Demo.Tests/HttpServerSim.Demo.Tests.csproj
```

## Running the app in Docker

The CI pipeline builds a Docker image and runs integration tests against a containerized instance of `HttpServerSim.App`.

### Build the image

```bash
docker build -t http-server-sim-build -f Dockerfile .
```

Build with verbose output and without using the Docker cache:

```bash
docker build -t http-server-sim-build --progress=plain --no-cache -f Dockerfile .
```

### Run a container from the image

Run interactively:

```bash
docker run -i -t http-server-sim-build
```

Run interactively and remove the container when it exits:

```bash
docker run -i -t --rm http-server-sim-build
```

Override the entrypoint with `bash`:

```bash
docker run -i -t --rm --entrypoint "/bin/bash" http-server-sim-build
```

Expose the simulator ports and open a shell:

```bash
docker run -i -t --rm -p 5000:5000 -p 5001:5001 --entrypoint "/bin/bash" http-server-sim-build
```

Expose the simulator ports and run the default entrypoint:

```bash
docker run -i -t --rm -p 5000:5000 -p 5001:5001 http-server-sim-build
```

## Building NuGet packages

Build packages from all projects:

```bash
dotnet pack --include-source --include-symbols --no-build
```

Build packages from all projects and set the assembly version and package version:

```bash
dotnet pack --include-source --include-symbols -p:PackageVersion=0.12.0 -p:Version=0.12.0 --output ./nupkg
```

Build a package for `HttpServerSim.App` only:

```bash
dotnet pack ./HttpServerSim.App/HttpServerSim.App.csproj --include-source --include-symbols -p:PackageVersion=0.12.0 -p:Version=0.12.0 --output ./HttpServerSim.App/nupkg
```

Build a local release package for testing:

```bash
dotnet build /warnaserror --configuration Release -p:Version=1.0.0.1 -p:PackageVersion=1.0.0.1 -p:PackageOutputPath=./nupkg
```

For more information about package source metadata, see [Source Link](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/sourcelink).

## Installing a local build as a global tool

Install a locally built package as a global .NET tool:

```bash
dotnet tool install -g --add-source ./HttpServerSim.App/nupkg http-server-sim --version 1.0.0.1
```

List installed global tools:

```bash
dotnet tool list -g
```

Remove the global tool:

```bash
dotnet tool uninstall http-server-sim -g
```
