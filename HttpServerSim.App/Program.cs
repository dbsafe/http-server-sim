using HttpServerSim;
using System.Reflection;

Console.WriteLine($"HttpServerSim version: {Assembly.GetExecutingAssembly().GetName().Version}");
var apiHttpSimServer = new ApiHttpSimServer(args);
apiHttpSimServer.Run();
