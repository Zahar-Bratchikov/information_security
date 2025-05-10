using multi_threaded_hashing.Services.Interfaces;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace multi_threaded_hashing.Services
{
    public class HashService : IHashService
    {
        private readonly Dictionary<Models.HashAlgorithm, System.Security.Cryptography.HashAlgorithm> _hashAlgorithms = new();

        public HashService()
        {
            InitializeHashAlgorithms();
        }

        private void InitializeHashAlgorithms()
        {
            _hashAlgorithms[Models.HashAlgorithm.MD5] = MD5.Create();
            _hashAlgorithms[Models.HashAlgorithm.SHA1] = SHA1.Create();
            _hashAlgorithms[Models.HashAlgorithm.SHA256] = SHA256.Create();
            _hashAlgorithms[Models.HashAlgorithm.SHA512] = SHA512.Create();
        }

        // Синхронный вариант для CPU-bound задач
        public string ComputeHashStringSync(string input, Models.HashAlgorithm algorithm)
        {
            using var hashAlgorithm = GetHashAlgorithm(algorithm);
            var hash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        public async Task<byte[]> ComputeHashAsync(string input, Models.HashAlgorithm algorithm)
        {
            // Task.Run только для синхронного CPU-bound кода
            return await Task.Run(() => {
                using var hashAlgorithm = GetHashAlgorithm(algorithm);
                return hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
            });
        }

        public async Task<string> ComputeHashStringAsync(string input, Models.HashAlgorithm algorithm)
        {
            // Task.Run только для синхронного CPU-bound кода
            return await Task.Run(() => ComputeHashStringSync(input, algorithm));
        }

        public async Task<byte[][]> ComputeHashesParallelAsync(string[] inputs, Models.HashAlgorithm algorithm, int threadCount, CancellationToken cancellationToken = default)
        {
            if (inputs == null || inputs.Length == 0)
                return Array.Empty<byte[]>();

            int effectiveThreadCount = Math.Min(inputs.Length, threadCount);
            var results = new byte[inputs.Length][];

            await Task.Run(() =>
            {
                Parallel.For(0, inputs.Length, new ParallelOptions { MaxDegreeOfParallelism = effectiveThreadCount, CancellationToken = cancellationToken }, i =>
                {
                    ThreadAffinityHelper.SetThreadAffinity(i % Environment.ProcessorCount);
                    cancellationToken.ThrowIfCancellationRequested();
                    using var hashAlgorithm = GetHashAlgorithm(algorithm);
                    results[i] = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(inputs[i]));
                });
            }, cancellationToken);

            return results;
        }

        public async Task<string> ComputeHashAsync(string input, Models.HashAlgorithm algorithm, int threadCount, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            if (threadCount <= 1 || input.Length < 1000)
            {
                return await Task.Run(() => ComputeHashStringSync(input, algorithm), cancellationToken);
            }

            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] result = Array.Empty<byte>();
            int chunkSize = inputBytes.Length / threadCount;
            var chunkHashes = new byte[threadCount][];

            await Task.Run(() =>
            {
                Parallel.For(0, threadCount, new ParallelOptions { MaxDegreeOfParallelism = threadCount, CancellationToken = cancellationToken }, i =>
                {
                    ThreadAffinityHelper.SetThreadAffinity(i % Environment.ProcessorCount);
                    int startIndex = i * chunkSize;
                    int length = (i == threadCount - 1) ? inputBytes.Length - startIndex : chunkSize;
                    if (length <= 0) return;
                    byte[] chunk = new byte[length];
                    Array.Copy(inputBytes, startIndex, chunk, 0, length);
                    using var hashAlgorithm = GetHashAlgorithm(algorithm);
                    chunkHashes[i] = hashAlgorithm.ComputeHash(chunk);
                });
            }, cancellationToken);

            using var finalHashAlgorithm = GetHashAlgorithm(algorithm);
            var combinedBytes = chunkHashes.Where(h => h != null).SelectMany(h => h).ToArray();
            result = finalHashAlgorithm.ComputeHash(combinedBytes);
            return BitConverter.ToString(result).Replace("-", "").ToLower();
        }

        private System.Security.Cryptography.HashAlgorithm GetHashAlgorithm(Models.HashAlgorithm algorithm)
        {
            return algorithm switch
            {
                Models.HashAlgorithm.MD5 => MD5.Create(),
                Models.HashAlgorithm.SHA1 => SHA1.Create(),
                Models.HashAlgorithm.SHA256 => SHA256.Create(),
                Models.HashAlgorithm.SHA512 => SHA512.Create(),
                _ => throw new ArgumentException($"Неподдерживаемый алгоритм хэширования: {algorithm}")
            };
        }

        public void Dispose()
        {
            foreach (var algorithm in _hashAlgorithms.Values)
            {
                algorithm.Dispose();
            }
            _hashAlgorithms.Clear();
            GC.SuppressFinalize(this);
        }
    }
}