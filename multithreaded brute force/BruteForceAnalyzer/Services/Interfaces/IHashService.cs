using System.Threading.Tasks;
using BruteForceAnalyzer.Models;
using System.Diagnostics;
using System.Threading;

namespace BruteForceAnalyzer.Services.Interfaces
{
    public interface IHashService : IDisposable
    {
        Task<byte[]> ComputeHashAsync(string input, HashAlgorithm algorithm);
        Task<string> ComputeHashStringAsync(string input, HashAlgorithm algorithm);
        Task<byte[][]> ComputeHashesParallelAsync(string[] inputs, HashAlgorithm algorithm, int threadCount);

        // Новый метод с поддержкой указания числа потоков и CancellationToken
        Task<string> ComputeHashAsync(string input, HashAlgorithm algorithm, int threadCount, CancellationToken cancellationToken);
    }
} 