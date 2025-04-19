using System;
using BruteForceAnalyzer.Services.Interfaces;

namespace BruteForceAnalyzer.Services
{
    public class ConsoleLogger : ILogger
    {
        public void LogInformation(string message)
        {
            Console.WriteLine($"[INFO] {message}");
        }

        public void LogWarning(string message)
        {
            Console.WriteLine($"[WARN] {message}");
        }

        public void LogError(string message, Exception? exception = null)
        {
            Console.WriteLine($"[ERROR] {message}");
            if (exception != null)
            {
                Console.WriteLine($"Exception: {exception.Message}");
                Console.WriteLine($"Stack trace: {exception.StackTrace}");
            }
        }
    }
} 