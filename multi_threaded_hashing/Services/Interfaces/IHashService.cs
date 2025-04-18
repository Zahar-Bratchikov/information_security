using System;
using System.Threading;
using System.Threading.Tasks;
using multi_threaded_hashing.Models;

namespace multi_threaded_hashing.Services.Interfaces
{
    public interface IHashService : IDisposable
    {
        Task<byte[]> ComputeHashAsync(string input, HashAlgorithm algorithm);
        Task<string> ComputeHashStringAsync(string input, HashAlgorithm algorithm);
        Task<byte[][]> ComputeHashesParallelAsync(string[] inputs, HashAlgorithm algorithm, int threadCount);
        Task<string> ComputeHashAsync(string input, HashAlgorithm algorithm, int threadCount, CancellationToken cancellationToken);
    }
}