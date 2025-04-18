using multi_threaded_hashing.Services.Interfaces;

namespace multi_threaded_hashing.Services
{
    public class ConsoleLogger : ILogger
    {
        public void LogInfo(string message)
        {
            Log(message, ConsoleColor.White);
        }

        public void LogWarning(string message)
        {
            Log(message, ConsoleColor.Yellow);
        }

        public void LogError(string message)
        {
            Log(message, ConsoleColor.Red);
        }

        public void LogSuccess(string message)
        {
            Log(message, ConsoleColor.Green);
        }

        private void Log(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
            Console.ResetColor();
        }
    }
}