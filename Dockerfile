FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
# Copy everything
COPY . ./
RUN dotnet build -c Release

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 5000
EXPOSE 5001
WORKDIR /app
COPY --from=build /app/HttpServerSim.App/bin/Release/net8.0/ /app
COPY --from=build /app/HttpServerSim.App/entrypoint.sh /app
RUN chmod +x /app/entrypoint.sh

# Using a script
# ENTRYPOINT ["/app/entrypoint.sh"]

# Using http-server-sim
ENTRYPOINT ["dotnet", "HttpServerSim.App.dll", "--Rules", "rules.json", "--ControlUrl", "http://*:5001", "--Url", "http://*:5000" ]

# To use an interactive session with command 'docker run -i -t http-server-sim-build'
# ENTRYPOINT ["/bin/bash"] 
