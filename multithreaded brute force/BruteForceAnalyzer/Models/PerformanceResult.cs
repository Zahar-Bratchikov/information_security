using System;

namespace BruteForceAnalyzer.Models
{
    public class PerformanceResult
    {
        public required string DeviceName { get; set; }
        public int ThreadCount { get; set; }
        public double HashesPerSecond { get; set; } // хешей/сек
        public string Algorithm { get; set; } = string.Empty;
        public int TestNumber { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
} 