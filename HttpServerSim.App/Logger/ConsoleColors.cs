// Ignore Spelling: App

namespace HttpServerSim;

// Ref: 
// https://stackoverflow.com/questions/2743260/is-it-possible-to-write-to-the-console-in-colour-in-net
// https://en.wikipedia.org/wiki/ANSI_escape_code
internal static class ConsoleColors
{
    public static string NormalColor = Console.IsOutputRedirected ? "" : "\x1b[39m";
    public static string RequestColor = Console.IsOutputRedirected ? "" : "\x1b[92m";
    public static string ResponseColor = Console.IsOutputRedirected ? "" : "\x1b[96m";
    public static string Bold = Console.IsOutputRedirected ? "" : "\x1b[1m";
    public static string NoBold = Console.IsOutputRedirected ? "" : "\x1b[22m";
    public static string Underline   = Console.IsOutputRedirected ? "" : "\x1b[4m";
    public static string NoUnderline = Console.IsOutputRedirected ? "" : "\x1b[24m";
}
