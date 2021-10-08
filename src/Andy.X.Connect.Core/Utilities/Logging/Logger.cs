using System;

namespace Andy.X.Connect.Core.Utilities.Logging
{
    public static class Logger
    {
        public static void LogInformation(string log)
        {
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH-mm-ss} andyx-connect [info]     |   {log}");
        }
        public static void LogWarning(string log)
        {
            var generalColor = Console.ForegroundColor;
            Console.Write($"{DateTime.Now:yyyy-MM-dd HH-mm-ss} andyx-connect ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"[warning]");
            Console.ForegroundColor = generalColor;
            Console.WriteLine($"  |   {log}");
        }

        public static void LogError(string log)
        {
            var generalColor = Console.ForegroundColor;
            Console.Write($"{DateTime.Now:yyyy-MM-dd HH-mm-ss} andyx-connect ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"[error]");
            Console.ForegroundColor = generalColor;
            Console.WriteLine($"    |   {log}");
        }
    }
}