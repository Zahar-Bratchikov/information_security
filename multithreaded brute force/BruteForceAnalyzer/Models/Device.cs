namespace BruteForceAnalyzer.Models
{
    public class Device
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public bool IsAvailable { get; set; }
        public int PerformanceScore { get; set; }
    }
} 