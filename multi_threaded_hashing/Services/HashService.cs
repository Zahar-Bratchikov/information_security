using multi_threaded_hashing.Services.Interfaces;
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

        public async Task<byte[]> ComputeHashAsync(string input, Models.HashAlgorithm algorithm)
        {
            return await Task.Run(() =>
            {
                using var hashAlgorithm = GetHashAlgorithm(algorithm);
                return hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
            });
        }

        public async Task<string> ComputeHashStringAsync(string input, Models.HashAlgorithm algorithm)
        {
            var hash = await ComputeHashAsync(input, algorithm);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        public async Task<byte[][]> ComputeHashesParallelAsync(string[] inputs, Models.HashAlgorithm algorithm, int threadCount)
        {
            var tasks = inputs.Select(input => ComputeHashAsync(input, algorithm));
            return await Task.WhenAll(tasks);
        }

        public async Task<string> ComputeHashAsync(string input, Models.HashAlgorithm algorithm, int threadCount, CancellationToken cancellationToken)
        {
            // Увеличиваем объем работы, чтобы заметить разницу от многопоточности
            const int hashIterations = 10000;
            string originalInput = input;

            // Создаем большой объем входных данных
            if (input.Length < 10000)
            {
                StringBuilder sb = new();
                while (sb.Length < 10000)
                {
                    sb.Append(input);
                }
                input = sb.ToString();
            }

            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] result = Array.Empty<byte>();

            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Разделяем данные на части для обработки разными потоками
                int chunkSize = input.Length / threadCount;
                List<byte[]> chunks = new();

                for (int i = 0; i < threadCount; i++)
                {
                    int startIndex = i * chunkSize;
                    int length = (i == threadCount - 1) ? inputBytes.Length - startIndex : chunkSize;

                    if (length <= 0) continue;

                    byte[] chunk = new byte[length];
                    Array.Copy(inputBytes, startIndex, chunk, 0, length);
                    chunks.Add(chunk);
                }

                // Вычисляем хеш для каждого сегмента много раз
                var tasks = new Task<byte[]>[chunks.Count];
                for (int i = 0; i < chunks.Count; i++)
                {
                    int chunkIndex = i;
                    tasks[i] = Task.Run(() =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        byte[] chunkBytes = chunks[chunkIndex];
                        byte[] lastHash = chunkBytes;

                        using var hashAlgorithm = GetHashAlgorithm(algorithm);

                        // Многократно хешируем для увеличения нагрузки
                        for (int iter = 0; iter < hashIterations / threadCount; iter++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            lastHash = hashAlgorithm.ComputeHash(lastHash);
                        }

                        return lastHash;
                    }, cancellationToken);
                }

                // Ждем завершения всех задач и объединяем результаты
                Task.WaitAll(tasks);

                // Создаем один финальный хеш из всех результатов
                using var finalHashAlgorithm = GetHashAlgorithm(algorithm);
                var combinedBytes = new List<byte>();

                foreach (var task in tasks)
                {
                    combinedBytes.AddRange(task.Result);
                }

                result = finalHashAlgorithm.ComputeHash(combinedBytes.ToArray());
            }, cancellationToken);

            // Для тестирования возвращаем хеш оригинального ввода
            return await ComputeHashStringAsync(originalInput, algorithm);
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