# Development

## Executing Tests

### Test running the app locally

Start `HttpServerSim.App` without debugger and execute the tests.

### Test running the app in a container

This is the way used in the build.</p>

Build image:</p> `docker build -t http-server-sim-build -f Dockerfile .`</p>
Build image with verbose output:</p> `docker build -t http-server-sim-build --progress=plain --no-cache -f Dockerfile .`</p>

Run container from image</p>
	- interactive
	`docker run -i -t http-server-sim-build`</p>
	- interactive, remove when done
	`docker run -i -t --rm http-server-sim-build`</p>
	- override ENTRYPOINT with bash
	`docker run -i -t --rm --entrypoint "/bin/bash" http-server-sim-build `</p>
	- mapp ports</p>
	`docker run -i -t --rm -p 8080:8080 -p 8090:8090 --entrypoint "/bin/bash" http-server-sim-build`</p>
	`docker run -i -t --rm -p 8080:8080 -p 8090:8090 http-server-sim-build`</p>

## Building a NuGet package

Builds packages from all projects:</p>
`dotnet pack --include-source --include-symbols --no-build`

Builds packages from all projects setting the version in the dll and in the package:</p>
`dotnet pack --include-source --include-symbols -p:PackageVersion=0.6.0 -p:Version=0.6.0 --output D:\LocalNuget`

Builds one package for one project:</p>
`dotnet pack .\HttpServerSim.App\HttpServerSim.App.csproj --include-source --include-symbols -p:PackageVersion=0.2.0 -p:Version=0.2.0 --output C:\LocalNuget`
`dotnet pack .\HttpServerSim.App\HttpServerSim.App.csproj --include-source --include-symbols -p:PackageVersion=0.8.0 -p:Version=0.8.0 --output ./HttpServerSim.App/nupkg`

Read more here
[Source Link](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/sourcelink)


### Install http-server-sim as global tool

#### Install last version
`dotnet tool install -g --add-source ./HttpServerSim.App/nupkg HttpServerSim.App`

### List installed dotnet tools

`dotnet tool list -g`

### Remove http-server-sim

`dotnet tool uninstall httpserversim.app -g`
