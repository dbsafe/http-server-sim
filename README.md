# http-server-sim
Test HTTP Server to support integration test for applications that send HTTP requests to other components.

### Building a NuGet package

Builds packages from all projects:</p>
`dotnet pack --include-source --include-symbols --no-build`

Builds packages from all projects setting the version in the dll and in the package:</p>
`dotnet pack --include-source --include-symbols -p:PackageVersion=0.2.0 -p:Version=0.2.0 --output D:\LocalNuget`

Builds one package for one project:</p>
`dotnet pack .\HttpServerSim.App\HttpServerSim.App.csproj --include-source --include-symbols -p:PackageVersion=0.2.0 -p:Version=0.2.0 --output C:\LocalNuget`
`dotnet pack .\HttpServerSim.App\HttpServerSim.App.csproj --include-source --include-symbols -p:PackageVersion=0.5.0 -p:Version=0.5.0 --output ./HttpServerSim.App/nupkg`

Read more here
[Source Link](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/sourcelink)


### Install http-server-sim as global tool

#### Install last version
`dotnet tool install -g --add-source ./HttpServerSim.App/nupkg HttpServerSim.App`

### List installed dotnet tools

`dotnet tool list -g`

### Remove http-server-sim

`dotnet tool uninstall httpserversim.app -g`