using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BruteForceAnalyzer.Models;
using BruteForceAnalyzer.Services.Interfaces;

namespace BruteForceAnalyzer.Services
{
    public class BruteForceService : IBruteForceService
    {
        private readonly IHashService _hashService;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isRunning;
        private DateTime _startTime;
        private long _totalAttempts;
        private readonly object _lockObject = new object();

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
                var tasks = new List<Task<string>>();

                for (int length = settings.MinLength; length <= settings.MaxLength; length++)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        break;

                    var result = await BruteForceLength(alphabetArray, length, settings.TargetHash, settings.ThreadCount, settings.Algorithm, _cancellationTokenSource.Token);
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

        private async Task<string> BruteForceLength(char[] alphabet, int length, string targetHash, int threadCount, HashAlgorithm algorithm, CancellationToken cancellationToken)
        {
            var totalCombinations = (long)Math.Pow(alphabet.Length, length);
            var combinationsPerThread = totalCombinations / threadCount;

            var tasks = new List<Task<string>>();
            for (int i = 0; i < threadCount; i++)
            {
                var startIndex = i * combinationsPerThread;
                var endIndex = (i == threadCount - 1) ? totalCombinations : startIndex + combinationsPerThread;
                tasks.Add(Task.Run(() => BruteForceRange(alphabet, length, targetHash, algorithm, startIndex, endIndex, cancellationToken)));
            }

            var results = await Task.WhenAll(tasks);
            return results.FirstOrDefault(r => !string.IsNullOrEmpty(r)) ?? string.Empty;
        }

        private async Task<string> BruteForceRange(char[] alphabet, int length, string targetHash, HashAlgorithm algorithm, long startIndex, long endIndex, CancellationToken cancellationToken)
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

            try 
            {
                for (long i = startIndex; i < endIndex; i += batchSize)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    var batchEnd = Math.Min(i + batchSize, endIndex);
                    for (long j = i; j < batchEnd; j++)
                    {
                        // Генерация текущей комбинации
                        long temp = j;
                        for (int k = 0; k < length; k++)
                        {
                            current[k] = alphabet[(int)(temp % alphabetLength)];  // Приведение к int для безопасности
                            temp /= alphabetLength;
                        }

                        var attempt = new string(current);
                        var hash = await _hashService.ComputeHashStringAsync(attempt, algorithm);  // Используем строковую версию

                        if (hash.Equals(targetHash, StringComparison.OrdinalIgnoreCase))
                        {
                            return attempt;
                        }

                        attempts++;
                        if (attempts % 100 == 0) // Обновляем прогресс каждые 100 попыток
                        {
                            UpdateProgress(attempts, attempt);
                            attempts = 0;  // Сбрасываем счетчик после обновления прогресса
                        }
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

        private void UpdateProgress(long attempts, string currentAttempt)
        {
            lock (_lockObject)
            {
                _totalAttempts += attempts;
                
                // Оценка максимального числа попыток (64-битное значение для большего диапазона)
                long maxAttempts = 0;
                for (int len = 1; len <= 8; len++)  // Оценка для длин от 1 до 8
                {
                    maxAttempts += (long)Math.Pow(62, len);  // 62 - примерное количество символов в алфавите
                }
                
                // Ограничиваем прогресс до 100%
                int progress = (int)Math.Min(100, (_totalAttempts * 100) / Math.Max(1, maxAttempts));
                
                OnProgressChanged(new BruteForceProgressEventArgs
                {
                    Progress = progress,
                    CurrentAttempt = currentAttempt,
                    ElapsedTime = DateTime.Now - _startTime,
                    TotalAttempts = _totalAttempts
                });
            }
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