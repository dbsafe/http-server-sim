using HttpServerSim;
using HttpServerSim.App.Config;
using System.Reflection;

Console.WriteLine($"HttpServerSim version: {Assembly.GetExecutingAssembly().GetName().Version}");

var isDebugMode = CommandLineHelper.IsDebugMode(args);
if (isDebugMode)
{
    Console.WriteLine("Executing in debug mode");
    Console.WriteLine($"CommandLine: {Environment.CommandLine}");
    Console.WriteLine($"ProcessPath: {Environment.ProcessPath}");
    Console.WriteLine($"CurrentDirectory: {Environment.CurrentDirectory}");
}

if (CommandLineHelper.IsHelpMode(args) || !AppConfigLoader.TryLoadAppConfig(args, isDebugMode, out AppConfig? appConfig))
{
    AppConfigLoader.PrintHelp();
    return;
}

appConfig = appConfig ?? throw new Exception($"{nameof(appConfig)} should not be null");
appConfig.IsDebugMode = isDebugMode;

try
{
    var apiHttpSimServer = new ApiHttpSimServer(args, appConfig);
    apiHttpSimServer.Run();
}
catch (Exception ex)
{
    var error = isDebugMode ? ex.ToString() : ex.Message;
    Console.WriteLine(error);
}
