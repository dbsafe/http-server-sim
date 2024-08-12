using EndpointToCSV.Models;
using System.Net.Http.Json;

namespace EndpointToCSV
{
    public class EndpointToCSVCore(string url, string output)
    {
        private static readonly HttpClient httpClient = new();

        public void Run()
        {
            var path = Path.IsPathRooted(output) ? output : Path.Combine(Environment.CurrentDirectory, output);

            Console.WriteLine($"Sending request to {url}");
            var customers = httpClient.GetFromJsonAsync<IEnumerable<Customer>>(url).Result ?? throw new Exception("Null is not expected");

            Console.WriteLine($"Writing data to {path}");
            using var sw = new StreamWriter(path);
            sw.Write(Customer.CSVHeader);
            
            foreach (var customer in customers)
            {
                sw.WriteLine();
                sw.Write(customer.CSVData);
            }
        }
    }
}
