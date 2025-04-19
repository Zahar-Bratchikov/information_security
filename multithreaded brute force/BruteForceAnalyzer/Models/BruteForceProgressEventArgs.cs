using System;

namespace BruteForceAnalyzer.Models
{
    public class BruteForceProgressEventArgs : EventArgs
    {
        public int Progress { get; set; }
        public string CurrentAttempt { get; set; } = string.Empty;
        public TimeSpan ElapsedTime { get; set; }
        public long TotalAttempts { get; set; }
    }
} 