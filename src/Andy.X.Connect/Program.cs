using Andy.X.Connect.Core.Services;
using Andy.X.Connect.Core.Utilities.Logging;
using System;

namespace Andy.X.Connect
{
    class Program
    {
        static void Main(string[] args)
        {
            GlobalService globalService;
            Console.WriteLine("Buildersoft Andy X Connect");

            globalService = new GlobalService();
            Logger.LogInformation("services are ready");


            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
