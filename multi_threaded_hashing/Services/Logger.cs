using multi_threaded_hashing.Services.Interfaces;

namespace multi_threaded_hashing.Services
{
    public class Logger : ILogger
    {
        private readonly ILogger _consoleLogger;
        private readonly ILogger _fileLogger;

        public Logger(ILogger consoleLogger, ILogger fileLogger)
        {
            _consoleLogger = consoleLogger ?? throw new ArgumentNullException(nameof(consoleLogger));
            _fileLogger = fileLogger ?? throw new ArgumentNullException(nameof(fileLogger));
        }

        public void LogInfo(string message)
        {
            _consoleLogger.LogInfo(message);
            _fileLogger.LogInfo(message);
        }

        public void LogWarning(string message)
        {
            _consoleLogger.LogWarning(message);
            _fileLogger.LogWarning(message);
        }

        public void LogError(string message)
        {
            _consoleLogger.LogError(message);
            _fileLogger.LogError(message);
        }

        public void LogSuccess(string message)
        {
            _consoleLogger.LogSuccess(message);
            _fileLogger.LogSuccess(message);
        }
    }
}