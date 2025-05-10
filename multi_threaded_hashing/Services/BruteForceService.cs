using multi_threaded_hashing.Models;
using multi_threaded_hashing.Services.Interfaces;
using System.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace multi_threaded_hashing.Services
{
    public class BruteForceService : IBruteForceService
    {
        private readonly IHashService _hashService;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isRunning;
        private DateTime _startTime;
        private long _totalAttempts;
        private readonly object _lockObject = new();

        public event EventHandler<BruteForceProgressEventArgs>? ProgressChanged;

        public BruteForceService(IHashService hashService)
        {
            _hashService = hashService ?? throw new ArgumentNullException(nameof(hashService));
        }

        public async Task<string> StartBruteForceAsync(BruteForceSettings settings, CancellationToken cancellationToken)
        {
            if (_isRunning)
                throw new InvalidOperationException("Брутфорс уже запущен");

            if (string.IsNullOrEmpty(settings.Alphabet))
                throw new ArgumentException("Алфавит не может быть пустым", nameof(settings));

            if (settings.MinLength <= 0 || settings.MaxLength < settings.MinLength)
                throw new ArgumentException("Некорректные значения длины пароля", nameof(settings));

            if (settings.ThreadCount <= 0)
                throw new ArgumentException("Количество потоков должно быть больше 0", nameof(settings));

            _isRunning = true;
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _startTime = DateTime.Now;
            _totalAttempts = 0;

            try
            {
                var alphabetArray = settings.Alphabet.Distinct().ToArray();

                for (int length = settings.MinLength; length <= settings.MaxLength; length++)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        break;

                    var result = await BruteForceLength(
                        alphabetArray,
                        length,
                        settings.TargetHash,
                        settings.ThreadCount,
                        settings.Algorithm,
                        _cancellationTokenSource.Token);

                    if (!string.IsNullOrEmpty(result))
                    {
                        return result;
                    }
                }

                return string.Empty;
            }
            finally
            {
                _isRunning = false;
                _cancellationTokenSource?.Dispose();
            }
        }

        private async Task<string> BruteForceLength(
            char[] alphabet,
            int length,
            string targetHash,
            int threadCount,
            HashAlgorithm algorithm,
            CancellationToken cancellationToken)
        {
            var totalCombinations = (long)Math.Pow(alphabet.Length, length);

            if (threadCount <= 1)
            {
                return await Task.Run(() => BruteForceRange(alphabet, length, targetHash, algorithm, 0, totalCombinations, cancellationToken));
            }

            int effectiveThreadCount = OptimizeThreadCount(threadCount, totalCombinations);
            var combinationsPerThread = totalCombinations / effectiveThreadCount;
            var tasks = new List<Task<string>>();
            var startEvent = new System.Threading.ManualResetEventSlim(false);

            for (int i = 0; i < effectiveThreadCount; i++)
            {
                var startIndex = i * combinationsPerThread;
                var endIndex = (i == effectiveThreadCount - 1) ? totalCombinations : startIndex + combinationsPerThread;
                int coreIndex = i % System.Environment.ProcessorCount;
                tasks.Add(Task.Run(() => {
                    ThreadAffinityHelper.SetThreadAffinity(coreIndex);
                    startEvent.Wait(); // Ждём сигнала старта
                    return BruteForceRange(
                        alphabet,
                        length,
                        targetHash,
                        algorithm,
                        startIndex,
                        endIndex,
                        cancellationToken);
                }));
            }

            // Все потоки готовы, даём сигнал на старт
            startEvent.Set();

            while (tasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(tasks);
                tasks.Remove(completedTask);
                var result = await completedTask;
                if (!string.IsNullOrEmpty(result))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return result;
                }
            }

            return string.Empty;
        }

        private int OptimizeThreadCount(int requestedThreadCount, long totalCombinations)
        {
            // Если комбинаций меньше, чем 1000 на поток, уменьшаем количество потоков
            if (totalCombinations < requestedThreadCount * 1000)
            {
                requestedThreadCount = Math.Max(1, (int)(totalCombinations / 1000));
            }

            // Используем не больше потоков, чем доступно ядер процессора + 1-2 потока
            return Math.Min(requestedThreadCount, Environment.ProcessorCount + 1);
        }

        private string BruteForceRange(
            char[] alphabet,
            int length,
            string targetHash,
            HashAlgorithm algorithm,
            long startIndex,
            long endIndex,
            CancellationToken cancellationToken)
        {
            // Проверяем на возможное переполнение
            if (length > 13)  // При длине 14 символов и алфавите из 62 символов уже превышается long.MaxValue
            {
                length = 13;  // Ограничиваем длину для предотвращения переполнения
            }

            var current = new char[length];
            var alphabetLength = alphabet.Length;
            var batchSize = 1000; // Размер пакета для обработки
            var attempts = 0L;
            var lastProgressUpdateTime = DateTime.Now;

            try
            {
                for (long i = startIndex; i < endIndex; i += batchSize)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    var batchEnd = Math.Min(i + batchSize, endIndex);
                    for (long j = i; j < batchEnd; j++)
                    {
                        GenerateCombination(j, current, alphabet, alphabetLength);

                        var attempt = new string(current);
                        // Используем синхронный вариант для CPU-bound
                        var hash = _hashService.ComputeHashStringSync(attempt, algorithm);
                        if (hash.Equals(targetHash, StringComparison.OrdinalIgnoreCase))
                        {
                            return attempt;
                        }

                        attempts++;
                    }

                    // Обновляем прогресс не чаще, чем раз в 100 мс
                    var now = DateTime.Now;
                    if ((now - lastProgressUpdateTime).TotalMilliseconds > 100)
                    {
                        UpdateProgress(attempts, new string(current));
                        attempts = 0;
                        lastProgressUpdateTime = now;
                    }
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку и продолжаем перебор
                Console.WriteLine($"Ошибка в BruteForceRange: {ex.Message}");
            }

            // Обновляем прогресс с оставшимися попытками
            if (attempts > 0)
            {
                UpdateProgress(attempts, new string(current));
            }

            return string.Empty;
        }

        private static void GenerateCombination(long index, char[] current, char[] alphabet, int alphabetLength)
        {
            // Генерация текущей комбинации
            for (int k = 0; k < current.Length; k++)
            {
                current[k] = alphabet[(int)(index % alphabetLength)];  // Приведение к int для безопасности
                index /= alphabetLength;
            }
        }

        private void UpdateProgress(long attempts, string currentAttempt)
        {
            lock (_lockObject)
            {
                _totalAttempts += attempts;

                // Оценка максимального числа попыток (64-битное значение для большего диапазона)
                long maxAttempts = EstimateMaxAttempts();

                // Ограничиваем прогресс до 100%
                int progress = (int)Math.Min(100, _totalAttempts * 100 / Math.Max(1, maxAttempts));

                var args = new BruteForceProgressEventArgs
                {
                    Progress = progress,
                    CurrentAttempt = currentAttempt,
                    ElapsedTime = DateTime.Now - _startTime,
                    TotalAttempts = _totalAttempts
                };

                // Отправляем событие в потоке UI
                Application.Current.Dispatcher.Invoke(() => OnProgressChanged(args));
            }
        }

        private long EstimateMaxAttempts()
        {
            long maxAttempts = 0;
            for (int len = 1; len <= 8; len++)  // Оценка для длин от 1 до 8
            {
                maxAttempts += (long)Math.Pow(62, len);  // 62 - примерное количество символов в алфавите
            }
            return maxAttempts;
        }

        public void StopBruteForce()
        {
            _cancellationTokenSource?.Cancel();
        }

        protected virtual void OnProgressChanged(BruteForceProgressEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }
    }
}