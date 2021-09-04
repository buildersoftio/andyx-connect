using Andy.X.Connect.Core.Services;
using System;

namespace Andy.X.Connect
{
    class Program
    {
        static void Main(string[] args)
        {
            GlobalService globalService;
            Console.WriteLine("Buildersoft");
            Console.WriteLine("Buildersoft Andy X");
            Console.WriteLine("Buildersoft Andy X Connect");
            Console.WriteLine("---------------------------------------------------");

            globalService = new GlobalService();
            Console.WriteLine("ANDYX-CONNECT|ready");

            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
