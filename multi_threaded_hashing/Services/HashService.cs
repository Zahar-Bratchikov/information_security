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

        public async Task<byte[][]> ComputeHashesParallelAsync(string[] inputs, Models.HashAlgorithm algorithm, int threadCount, CancellationToken cancellationToken = default)
        {
            if (inputs == null || inputs.Length == 0)
                return Array.Empty<byte[]>();
            
            // Если входных данных меньше чем threadCount, используем количество потоков равное количеству строк
            int effectiveThreadCount = Math.Min(inputs.Length, threadCount);
            
            // Если запрошен только один поток или всего один элемент, используем простой подход
            if (effectiveThreadCount <= 1)
            {
                var tasks = inputs.Select(input => ComputeHashAsync(input, algorithm));
                return await Task.WhenAll(tasks);
            }
            
            // Разделяем входные данные на группы для параллельной обработки
            var batches = new List<string[]>();
            int batchSize = (inputs.Length + effectiveThreadCount - 1) / effectiveThreadCount; // Округление вверх
            
            for (int i = 0; i < inputs.Length; i += batchSize)
            {
                int count = Math.Min(batchSize, inputs.Length - i);
                var batch = new string[count];
                Array.Copy(inputs, i, batch, 0, count);
                batches.Add(batch);
            }
            
            // Создаем задачи для каждой группы
            var batchTasks = new List<Task<List<byte[]>>>();
            foreach (var batch in batches)
            {
                batchTasks.Add(Task.Run(async () =>
                {
                    var results = new List<byte[]>();
                    foreach (var input in batch)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        
                        results.Add(await Task.Run(() =>
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            using var hashAlgorithm = GetHashAlgorithm(algorithm);
                            return hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
                        }, cancellationToken));
                    }
                    return results;
                }, cancellationToken));
            }
            
            // Ждем завершения всех групп и объединяем результаты
            var batchResults = await Task.WhenAll(batchTasks);
            
            // Объединяем результаты всех групп в один массив
            var results = new List<byte[]>();
            foreach (var batchResult in batchResults)
            {
                results.AddRange(batchResult);
            }
            
            return results.ToArray();
        }

        public async Task<string> ComputeHashAsync(string input, Models.HashAlgorithm algorithm, int threadCount, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            
            // Если запрошен только один поток или строка слишком короткая, используем обычный метод
            if (threadCount <= 1 || input.Length < 1000)
            {
                return await ComputeHashStringAsync(input, algorithm);
            }

            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] result = Array.Empty<byte>();

            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Разделяем данные на части для обработки разными потоками
                int chunkSize = inputBytes.Length / threadCount;
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

                // Вычисляем хеш для каждого сегмента параллельно
                var tasks = new Task<byte[]>[chunks.Count];
                for (int i = 0; i < chunks.Count; i++)
                {
                    int chunkIndex = i;
                    tasks[i] = Task.Run(() =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        using var hashAlgorithm = GetHashAlgorithm(algorithm);
                        return hashAlgorithm.ComputeHash(chunks[chunkIndex]);
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

            // Возвращаем результат финального хеширования в виде строки
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