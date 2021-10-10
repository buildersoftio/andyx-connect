using Andy.X.Connect.Core.Services;
using Andy.X.Connect.Core.Utilities.Logging;
using System;
using System.Diagnostics;

namespace Andy.X.Connect
{
    class Program
    {
        static void Main(string[] args)
        {
            GlobalService globalService;
            LoggerSink loggerSink;
            // Sinking Console logs to log files
            loggerSink = new LoggerSink();
            loggerSink.InitializeSink();

            Logger.ShowWelcomeTest();

            globalService = new GlobalService();
            Logger.LogInformation("Andy X Connect is ready");


            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
