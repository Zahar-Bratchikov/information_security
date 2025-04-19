namespace BruteForceAnalyzer.Models
{
    public enum HashAlgorithm
    {
        MD5,
        SHA1,
        SHA256
    }

    public class HashFunction
    {
        public required string Name { get; set; }
        public HashAlgorithm Algorithm { get; set; }
        public int BlockSize { get; set; }
        public int OutputSize { get; set; }
    }
} 