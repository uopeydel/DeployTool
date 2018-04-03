using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace TCP
{
    public class Program
    {
        public static IConfiguration Configuration { get; set; }
        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
            var host = Configuration["host"];
            var portStr = Configuration["port"];
            var isPort = Int32.TryParse(portStr, out int port);
            if (!isPort)
            {
                throw new Exception($"{portStr} is not number");
            }

            Console.WriteLine($"  {host}:{port}");

            Console.Write("server (y) : ");

            var mode = Console.ReadLine();

            if (mode.ToLowerInvariant().Equals("y"))
            {
                Console.WriteLine("server");
                var serv = new TcpServerHelper(port);
                serv.Start();
            }
            else
            {
                Console.WriteLine("client");
                var client = new TcpClientHelper(host, port);
                client.Start();
            }

           
            Console.ReadKey();
        }
    }
}
