using System;

namespace BruteForceAnalyzer.Models
{
    public class BruteForceSettings
    {
        public required string TargetHash { get; set; }
        public HashAlgorithm Algorithm { get; set; }
        public int ThreadCount { get; set; }
        public required string Alphabet { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
    }
} 