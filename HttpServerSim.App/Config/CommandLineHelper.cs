// Ignore Spelling: App

namespace HttpServerSim.App.Config;

public static class CommandLineHelper
{
    public static bool IsDebugMode(string[] args) => GetValueFromArgs(args, "Logging:LogLevel:HttpServerSim")?.CompareTo("Debug") == 0;

    public static bool IsHelpMode(string[] args) => GetValueFromArgs(args, "Help")?.CompareTo("Help") == 0;

    public static string? GetValueFromArgs(string[] args, string name)
    {
        var list = ParseArgs(args);
        return list.FirstOrDefault(a => a.Key == name).Value;
    }

    public static List<KeyValuePair<string, string>> ParseArgs(string[] args)
    {
        var list = new List<KeyValuePair<string, string>>();

        string? lastKey = null;
        foreach (var arg in args)
        {
            if (arg.StartsWith("--"))
            {
                if (lastKey is not null)
                {
                    list.Add(new KeyValuePair<string, string>(lastKey, lastKey));
                }

                lastKey = arg[2..];
                continue;
            }

            if (lastKey is not null)
            {
                list.Add(new KeyValuePair<string, string>(lastKey, arg));
                lastKey = null;
            }
            else
            {
                list.Add(new KeyValuePair<string, string>(arg, arg));
            }
        }

        if (lastKey is not null)
        {
            list.Add(new KeyValuePair<string, string>(lastKey, lastKey));
        }

        return list;
    }
}

