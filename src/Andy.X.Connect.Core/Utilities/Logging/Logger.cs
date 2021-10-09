using System;
using System.Diagnostics;

namespace Andy.X.Connect.Core.Utilities.Logging
{
    public static class Logger
    {
        public static void LogInformation(string log)
        {
            Trace.WriteLine($"{DateTime.Now:yyyy-MM-dd HH-mm-ss} andyx-connect [info]     |   {log}");
        }

        public static void LogWarning(string log)
        {
            var generalColor = Console.ForegroundColor;
            Trace.Write($"{DateTime.Now:yyyy-MM-dd HH-mm-ss} andyx-connect ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Trace.Write($"[warning]");
            Console.ForegroundColor = generalColor;
            Trace.WriteLine($"  |   {log}");
        }

        public static void LogError(string log)
        {
            var generalColor = Console.ForegroundColor;
            Trace.Write($"{DateTime.Now:yyyy-MM-dd HH-mm-ss} andyx-connect ");
            Console.ForegroundColor = ConsoleColor.Red;
            Trace.Write($"[error]");
            Console.ForegroundColor = generalColor;
            Trace.WriteLine($"    |   {log}");
        }

        public static void LogError(string log, string logWithRed)
        {
            var generalColor = Console.ForegroundColor;
            Trace.Write($"{DateTime.Now:yyyy-MM-dd HH-mm-ss} andyx-connect ");
            Console.ForegroundColor = ConsoleColor.Red;
            Trace.Write($"[error]");
            Console.ForegroundColor = generalColor;
            Trace.Write($"    |   {log}");
            Console.ForegroundColor = ConsoleColor.Red;
            Trace.WriteLine(logWithRed);
            Console.ForegroundColor = generalColor;
        }
    }
}