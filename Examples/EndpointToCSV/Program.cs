// This demo application request data from a url and creates a CSV file.

using EndpointToCSV;

Console.WriteLine("EndpointToSCV");

if (args.Length != 2)
{
    Console.WriteLine($"Invalid or missing arguments.{Environment.NewLine}Expected: EndpointToSCV.exe <get-url> <output-cvs-file>");
    return;
}

var endpointToCSV = new EndpointToCSVCore(args[0], args[1]);
endpointToCSV.Run();
Console.WriteLine("Done");