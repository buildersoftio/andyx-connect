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

            Trace.WriteLine("Buildersoft Andy X Connect");
            Trace.WriteLine("Version 1.0.1-preview");
            Trace.WriteLine("Andy X Connect is an open source distributed platform for change data capture. Start it up, point it at your databases, and your apps can start responding to all of the inserts, updates, and deletes that other apps commit to your databases\n");

            globalService = new GlobalService();
            Logger.LogInformation("Andy X Connect is ready");


            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
