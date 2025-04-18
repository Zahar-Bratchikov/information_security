﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using multi_threaded_hashing.Models;
using multi_threaded_hashing.Services;
using multi_threaded_hashing.Services.Interfaces;
using multi_threaded_hashing.Commands;
using System.Windows;
using LiveCharts;
using LiveCharts.Wpf;
using System.Management;
using System.Runtime.InteropServices;
using LiveCharts.Configurations;

namespace multi_threaded_hashing.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IBruteForceService _bruteForceService;
        private readonly IHashService _hashService;
        private readonly IDeviceService _deviceService;
        private readonly ILogger _logger;
        private CancellationTokenSource _cancellationTokenSource;
        private string _targetHash = string.Empty;
        private string _result = string.Empty;
        private bool _isProcessing;
        private int _progress;
        private string _status = string.Empty;
        private string _currentAttempt = string.Empty;
        private TimeSpan _elapsedTime;
        private Models.HashAlgorithm _selectedAlgorithm;
        private int _threadCount;
        private string _alphabet = string.Empty;
        private int _minLength;
        private int _maxLength;
        private ObservableCollection<Models.HashAlgorithm> _availableAlgorithms = new();
        private string _inputText = string.Empty;
        private string _hashResult = string.Empty;
        private List<HashPerformanceResult> _hashingPerformanceResults = new();
        private ObservableCollection<Device> _availableDevices = new();
        private Device? _selectedDevice;
        private List<PerformanceResult> _performanceResults = new();
        private int _progressValue;
        private string _foundPassword = string.Empty;
        private string _logText = string.Empty;
        private double _attemptsPerSecond;
        private SeriesCollection _chartSeries = new SeriesCollection();
        private SeriesCollection _hashingChartSeries = new SeriesCollection();
        private string[] _hashingChartLabels = Array.Empty<string>();
        private string[] _chartLabels = Array.Empty<string>();
        private string _processorInfo = string.Empty;
        private string _processorCount = string.Empty;
        private string _osInfo = string.Empty;
        private ObservableCollection<int> _availableThreadCounts = new();
        private int _selectedThreadCount;
        private Func<double, string> _chartFormatter = value => value.ToString("N0");
        private Func<double, string> _hashingChartFormatter = value => value.ToString("N0");
        private List<BruteForcePerformanceResult> _bruteForcePerformanceResults = new();
        private SeriesCollection _bruteForceChartSeries = new SeriesCollection();
        private string[] _bruteForceChartLabels = Array.Empty<string>();
        private Func<double, string> _bruteForceChartFormatter = value => value.ToString("N0");

        public event EventHandler? PerformanceResultsUpdated;

        public MainViewModel(
            IBruteForceService bruteForceService,
            IHashService hashService,
            IDeviceService deviceService,
            ILogger logger)
        {
            _bruteForceService = bruteForceService;
            _hashService = hashService;
            _deviceService = deviceService;
            _logger = logger;
            _cancellationTokenSource = new CancellationTokenSource();

            // Подписываемся на события прогресса
            _bruteForceService.ProgressChanged += OnBruteForceProgressChanged;

            StartBruteForceCommand = new ActionCommand(StartBruteForce);
            StopBruteForceCommand = new ActionCommand(StopBruteForce);
            ClearCommand = new ActionCommand(ClearResults);
            ComputeHashCommand = new ActionCommand(ComputeHash);
            CopyHashCommand = new ActionCommand(CopyHashToClipboard);
            UseHashForBruteForceCommand = new ActionCommand(UseHashForBruteForce);
            RefreshDevicesCommand = new ActionCommand(RefreshDevices);
            StopHashingCommand = new ActionCommand(StopHashing);
            RunPerformanceTestCommand = new ActionCommand(RunPerformanceTest);
            ClearGraphsAndLogsCommand = new ActionCommand(ClearGraphsAndLogs);
            RunBruteForcePerformanceTestCommand = new ActionCommand(RunBruteForcePerformanceTest);

            InitializeSettings();
            InitializeSystemInfo();
        }

        public string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                OnPropertyChanged(nameof(InputText));
            }
        }

        public string HashResult
        {
            get => _hashResult;
            set
            {
                _hashResult = value;
                OnPropertyChanged(nameof(HashResult));
            }
        }

        public string TargetHash
        {
            get => _targetHash;
            set
            {
                _targetHash = value;
                OnPropertyChanged(nameof(TargetHash));
            }
        }

        public string Result
        {
            get => _result;
            set
            {
                _result = value;
                OnPropertyChanged(nameof(Result));
            }
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                _isProcessing = value;
                OnPropertyChanged(nameof(IsProcessing));
            }
        }

        public int Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public string CurrentAttempt
        {
            get => _currentAttempt;
            set
            {
                _currentAttempt = value;
                OnPropertyChanged(nameof(CurrentAttempt));
            }
        }

        public TimeSpan ElapsedTime
        {
            get => _elapsedTime;
            set
            {
                _elapsedTime = value;
                OnPropertyChanged(nameof(ElapsedTime));
            }
        }

        public Models.HashAlgorithm SelectedAlgorithm
        {
            get => _selectedAlgorithm;
            set
            {
                _selectedAlgorithm = value;
                OnPropertyChanged(nameof(SelectedAlgorithm));
            }
        }

        public int ThreadCount
        {
            get => _threadCount;
            set
            {
                _threadCount = value;
                OnPropertyChanged(nameof(ThreadCount));
            }
        }

        public string Alphabet
        {
            get => _alphabet;
            set
            {
                _alphabet = value;
                OnPropertyChanged(nameof(Alphabet));
            }
        }

        public int MinLength
        {
            get => _minLength;
            set
            {
                _minLength = value;
                OnPropertyChanged(nameof(MinLength));
            }
        }

        public int MaxLength
        {
            get => _maxLength;
            set
            {
                _maxLength = value;
                OnPropertyChanged(nameof(MaxLength));
            }
        }

        public ObservableCollection<Models.HashAlgorithm> AvailableAlgorithms
        {
            get => _availableAlgorithms;
            set
            {
                _availableAlgorithms = value;
                OnPropertyChanged(nameof(AvailableAlgorithms));
            }
        }

        public List<HashPerformanceResult> HashingPerformanceResults
        {
            get => _hashingPerformanceResults;
            set
            {
                _hashingPerformanceResults = value;
                OnPropertyChanged(nameof(HashingPerformanceResults));
            }
        }

        public ObservableCollection<Device> AvailableDevices
        {
            get => _availableDevices;
            set
            {
                _availableDevices = value;
                OnPropertyChanged(nameof(AvailableDevices));
            }
        }

        public Device? SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                _selectedDevice = value;
                OnPropertyChanged(nameof(SelectedDevice));
            }
        }

        public List<PerformanceResult> PerformanceResults
        {
            get => _performanceResults;
            set
            {
                _performanceResults = value;
                OnPropertyChanged(nameof(PerformanceResults));
                PerformanceResultsUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        public int ProgressValue
        {
            get => _progressValue;
            set
            {
                _progressValue = value;
                OnPropertyChanged(nameof(ProgressValue));
            }
        }

        public string FoundPassword
        {
            get => _foundPassword;
            set
            {
                _foundPassword = value;
                OnPropertyChanged(nameof(FoundPassword));
            }
        }

        public string LogText
        {
            get => _logText;
            set
            {
                _logText = value;
                OnPropertyChanged(nameof(LogText));
            }
        }

        public double AttemptsPerSecond
        {
            get => _attemptsPerSecond;
            set
            {
                _attemptsPerSecond = value;
                OnPropertyChanged(nameof(AttemptsPerSecond));
            }
        }

        public SeriesCollection ChartSeries
        {
            get => _chartSeries;
            set
            {
                _chartSeries = value;
                OnPropertyChanged(nameof(ChartSeries));
            }
        }

        public SeriesCollection HashingChartSeries
        {
            get => _hashingChartSeries;
            set
            {
                _hashingChartSeries = value;
                OnPropertyChanged(nameof(HashingChartSeries));
            }
        }

        public string[] HashingChartLabels
        {
            get => _hashingChartLabels;
            set
            {
                _hashingChartLabels = value;
                OnPropertyChanged(nameof(HashingChartLabels));
            }
        }

        public string[] ChartLabels
        {
            get => _chartLabels;
            set
            {
                _chartLabels = value;
                OnPropertyChanged(nameof(ChartLabels));
            }
        }

        public Func<double, string> ChartFormatter
        {
            get => _chartFormatter;
            set
            {
                _chartFormatter = value;
                OnPropertyChanged(nameof(ChartFormatter));
            }
        }

        public Func<double, string> HashingChartFormatter
        {
            get => _hashingChartFormatter;
            set
            {
                _hashingChartFormatter = value;
                OnPropertyChanged(nameof(HashingChartFormatter));
            }
        }

        public SeriesCollection BruteForceChartSeries
        {
            get => _bruteForceChartSeries;
            set
            {
                _bruteForceChartSeries = value;
                OnPropertyChanged(nameof(BruteForceChartSeries));
            }
        }

        public string[] BruteForceChartLabels
        {
            get => _bruteForceChartLabels;
            set
            {
                _bruteForceChartLabels = value;
                OnPropertyChanged(nameof(BruteForceChartLabels));
            }
        }

        public Func<double, string> BruteForceChartFormatter
        {
            get => _bruteForceChartFormatter;
            set
            {
                _bruteForceChartFormatter = value;
                OnPropertyChanged(nameof(BruteForceChartFormatter));
            }
        }

        public ICommand StartBruteForceCommand { get; }
        public ICommand StopBruteForceCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand ComputeHashCommand { get; }
        public ICommand CopyHashCommand { get; }
        public ICommand UseHashForBruteForceCommand { get; }
        public ICommand RefreshDevicesCommand { get; }
        public ICommand StopHashingCommand { get; }
        public ICommand RunPerformanceTestCommand { get; }
        public ICommand ClearGraphsAndLogsCommand { get; }
        public ICommand RunBruteForcePerformanceTestCommand { get; }

        public ObservableCollection<int> AvailableThreadCounts
        {
            get => _availableThreadCounts;
            set
            {
                _availableThreadCounts = value;
                OnPropertyChanged(nameof(AvailableThreadCounts));
            }
        }

        public int SelectedThreadCount
        {
            get => _selectedThreadCount;
            set
            {
                _selectedThreadCount = value;
                OnPropertyChanged(nameof(SelectedThreadCount));
            }
        }

        public string ProcessorInfo
        {
            get => _processorInfo;
            set
            {
                _processorInfo = value;
                OnPropertyChanged(nameof(ProcessorInfo));
            }
        }

        public string ProcessorCount
        {
            get => _processorCount;
            set
            {
                _processorCount = value;
                OnPropertyChanged(nameof(ProcessorCount));
            }
        }

        public string OsInfo
        {
            get => _osInfo;
            set
            {
                _osInfo = value;
                OnPropertyChanged(nameof(OsInfo));
            }
        }

        public List<BruteForcePerformanceResult> BruteForcePerformanceResults
        {
            get => _bruteForcePerformanceResults;
            set
            {
                _bruteForcePerformanceResults = value;
                OnPropertyChanged(nameof(BruteForcePerformanceResults));
            }
        }

        private void InitializeSettings()
        {
            AvailableAlgorithms = new ObservableCollection<Models.HashAlgorithm>(
                Enum.GetValues(typeof(Models.HashAlgorithm)).Cast<Models.HashAlgorithm>());

            SelectedAlgorithm = Models.HashAlgorithm.MD5;
            ThreadCount = Environment.ProcessorCount;
            Alphabet = "abcdefghijklmnopqrstuvwxyz0123456789";
            MinLength = 1;
            MaxLength = 8;

            // Инициализация доступных потоков
            var counts = new List<int>();
            for (int i = 1; i <= Environment.ProcessorCount; i++)
            {
                counts.Add(i);
            }
            AvailableThreadCounts = new ObservableCollection<int>(counts);
            SelectedThreadCount = Environment.ProcessorCount;
        }

        private void InitializeSystemInfo()
        {
            try
            {
                // Информация о процессоре
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        ProcessorInfo = obj["Name"]?.ToString() ?? "Неизвестно";
                        break;
                    }
                }

                // Количество ядер
                ProcessorCount = $"{Environment.ProcessorCount} логических ядер";

                // Операционная система
                OsInfo = RuntimeInformation.OSDescription;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при получении системной информации: {ex.Message}");
                ProcessorInfo = "Ошибка получения данных";
                ProcessorCount = "Ошибка получения данных";
                OsInfo = "Ошибка получения данных";
            }
        }

        private async void ComputeHash()
        {
            if (string.IsNullOrEmpty(InputText))
            {
                Status = "Введите текст для хэширования";
                LogText += "Ошибка: Введите текст для хэширования\n";
                return;
            }

            IsProcessing = true;
            Status = "Вычисление хэша...";
            LogText += $"Начато хэширование текста длиной {InputText.Length} символов\n";
            LogText += $"Количество потоков: {SelectedThreadCount}\n";

            try
            {
                // Создаем новый токен отмены для каждой операции
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = new CancellationTokenSource();
                }

                var startTime = DateTime.Now;
                string hash = await _hashService.ComputeHashAsync(
                    InputText,
                    SelectedAlgorithm,
                    SelectedThreadCount,
                    _cancellationTokenSource.Token);
                var endTime = DateTime.Now;

                var duration = (endTime - startTime).TotalMilliseconds;
                double hashesPerSecond = 1000.0 / duration;

                // Добавляем результат в список
                var result = new HashPerformanceResult
                {
                    ThreadCount = SelectedThreadCount,
                    Duration = duration,
                    HashesPerSecond = hashesPerSecond
                };

                // Обновляем список результатов, заменяя существующий для этого количества потоков
                var results = new List<HashPerformanceResult>(HashingPerformanceResults);
                var existingIndex = results.FindIndex(r => r.ThreadCount == SelectedThreadCount);
                if (existingIndex >= 0)
                {
                    results[existingIndex] = result;
                }
                else
                {
                    results.Add(result);
                }

                HashingPerformanceResults = results.OrderBy(r => r.ThreadCount).ToList();

                // Устанавливаем результат хэширования
                HashResult = hash;

                UpdateHashingChartSeries();

                Status = "Хэширование завершено";
                LogText += $"Хэширование завершено за {duration:F2} мс\n";
                LogText += $"Результат: {hash}\n";
                LogText += $"Скорость: {hashesPerSecond:F2} хешей/сек\n";
            }
            catch (OperationCanceledException)
            {
                Status = "Операция отменена";
                LogText += "Хэширование отменено пользователем\n";
            }
            catch (Exception ex)
            {
                Status = $"Ошибка: {ex.Message}";
                LogText += $"Ошибка при хэшировании: {ex.Message}\n";
                _logger.LogError($"Ошибка при хэшировании: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
                ProgressValue = 0;
            }
        }

        // Метод для тестирования производительности с разным числом потоков
        private async void RunPerformanceTest()
        {
            if (string.IsNullOrEmpty(InputText))
            {
                Status = "Введите текст для хэширования";
                LogText += "Ошибка: Введите текст для хэширования\n";
                return;
            }

            IsProcessing = true;
            Status = "Тестирование производительности...";
            LogText += $"Начато тестирование производительности хэширования\n";

            try
            {
                // Создаем новый токен отмены для каждой операции
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = new CancellationTokenSource();
                }

                var results = new List<HashPerformanceResult>();
                var maxThreads = Math.Min(Environment.ProcessorCount, 16); // Ограничиваем максимальное количество потоков

                // Выполняем хэширование с разным количеством потоков
                for (int threads = 1; threads <= maxThreads; threads++)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        break;

                    Status = $"Тестирование ({threads} потоков)...";
                    LogText += $"Тестирование: {threads} потоков\n";

                    // Несколько раз запускаем тест для получения более точных результатов
                    double totalDuration = 0;
                    int iterations = 3;

                    for (int i = 0; i < iterations; i++)
                    {
                        var startTime = DateTime.Now;
                        var hash = await _hashService.ComputeHashAsync(
                            InputText,
                            SelectedAlgorithm,
                            threads,
                            _cancellationTokenSource.Token);
                        var endTime = DateTime.Now;

                        var duration = (endTime - startTime).TotalMilliseconds;
                        totalDuration += duration;

                        // Небольшая задержка между тестами
                        await Task.Delay(50, _cancellationTokenSource.Token);
                    }

                    // Вычисляем среднее время
                    var averageDuration = totalDuration / iterations;
                    double hashesPerSecond = 1000.0 / averageDuration;

                    results.Add(new HashPerformanceResult
                    {
                        ThreadCount = threads,
                        Duration = averageDuration,
                        HashesPerSecond = hashesPerSecond
                    });

                    // Обновляем прогресс
                    ProgressValue = (int)((double)threads / maxThreads * 100);
                }

                HashingPerformanceResults = results;
                UpdateHashingChartSeries();

                Status = "Тестирование завершено";
                LogText += $"Тестирование завершено\n";

                // Определяем оптимальное количество потоков
                if (results.Count > 0)
                {
                    var optimalResult = results.OrderByDescending(r => r.HashesPerSecond).First();
                    LogText += $"Лучший результат: {optimalResult.ThreadCount} потоков - {optimalResult.HashesPerSecond:F2} хешей/сек\n";
                    LogText += $"Рекомендуемое количество потоков для этой задачи: {optimalResult.ThreadCount}\n";

                    // Устанавливаем оптимальное количество потоков
                    SelectedThreadCount = optimalResult.ThreadCount;
                }
            }
            catch (OperationCanceledException)
            {
                Status = "Операция отменена";
                LogText += "Тестирование отменено пользователем\n";
            }
            catch (Exception ex)
            {
                Status = $"Ошибка: {ex.Message}";
                LogText += $"Ошибка при тестировании: {ex.Message}\n";
                _logger.LogError($"Ошибка при тестировании производительности: {ex}");
            }
            finally
            {
                IsProcessing = false;
                ProgressValue = 0;
            }
        }

        private async void StartBruteForce()
        {
            if (string.IsNullOrEmpty(TargetHash))
            {
                Status = "Введите хеш для подбора";
                LogText += "Ошибка: Введите хеш для подбора\n";
                return;
            }

            if (string.IsNullOrEmpty(Alphabet))
            {
                Status = "Укажите алфавит для перебора";
                LogText += "Ошибка: Укажите алфавит для перебора\n";
                return;
            }

            if (MinLength <= 0 || MaxLength < MinLength)
            {
                Status = "Укажите корректные значения длины строки";
                LogText += "Ошибка: Некорректные значения длины строки\n";
                return;
            }

            IsProcessing = true;
            Status = "Подбор пароля...";
            Result = string.Empty;
            FoundPassword = string.Empty;
            Progress = 0;
            ProgressValue = 0;
            ElapsedTime = TimeSpan.Zero;
            _performanceResults.Clear();
            UpdateChartSeries();

            // Регистрируем начальное время для этого количества потоков
            var startThreadTime = DateTime.Now;

            LogText += $"Начато подбор пароля для хеша: {TargetHash}\n";
            LogText += $"Алгоритм: {SelectedAlgorithm}, Потоков: {ThreadCount}\n";
            LogText += $"Алфавит: {Alphabet} (длина {Alphabet.Length})\n";
            LogText += $"Диапазон длин: {MinLength} - {MaxLength}\n";

            try
            {
                var settings = new BruteForceSettings
                {
                    TargetHash = TargetHash,
                    Algorithm = SelectedAlgorithm,
                    ThreadCount = ThreadCount,
                    Alphabet = Alphabet,
                    MinLength = MinLength,
                    MaxLength = MaxLength
                };

                // Создаем новый токен отмены для каждой операции
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = new CancellationTokenSource();
                }

                var startTime = DateTime.Now;
                var result = await _bruteForceService.StartBruteForceAsync(settings, _cancellationTokenSource.Token);
                var endTime = DateTime.Now;
                var duration = endTime - startTime;

                // Добавляем результат производительности для построения графика
                AddBruteForcePerformanceResult(ThreadCount, duration.TotalMilliseconds);

                if (!string.IsNullOrEmpty(result))
                {
                    Result = $"Найден пароль: {result}";
                    FoundPassword = result;
                    Status = $"Брутфорс успешно завершен за {duration.ToString(@"hh\:mm\:ss")}";
                    LogText += $"Успех! Найден пароль: {result}\n";
                    LogText += $"Время подбора: {duration.ToString(@"hh\:mm\:ss")}\n";
                }
                else
                {
                    Result = "Пароль не найден";
                    Status = $"Брутфорс завершен без результата за {duration.ToString(@"hh\:mm\:ss")}";
                    LogText += "Пароль не найден. Проверьте параметры поиска.\n";
                    LogText += $"Время работы: {duration.ToString(@"hh\:mm\:ss")}\n";
                }

                // Обновляем метки для графика
                var labels = new string[_performanceResults.Count];
                for (int i = 0; i < _performanceResults.Count; i++)
                {
                    labels[i] = $"Проход {i + 1}";
                }
                ChartLabels = labels;
            }
            catch (OperationCanceledException)
            {
                Status = "Операция отменена";
                Result = "Отменено пользователем";
                LogText += "Брутфорс отменен пользователем\n";
            }
            catch (Exception ex)
            {
                Status = $"Ошибка: {ex.Message}";
                Result = "Произошла ошибка";
                LogText += $"Ошибка при подборе пароля: {ex.Message}\n";
                _logger.LogError($"Ошибка при подборе пароля: {ex}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private void StopBruteForce()
        {
            _cancellationTokenSource.Cancel();
            Status = "Остановка...";
        }

        private void ClearResults()
        {
            TargetHash = string.Empty;
            Result = string.Empty;
            Status = string.Empty;
            Progress = 0;
            CurrentAttempt = string.Empty;
            ElapsedTime = TimeSpan.Zero;
        }

        private void CopyHashToClipboard()
        {
            if (string.IsNullOrEmpty(HashResult))
            {
                Status = "Нет хэша для копирования";
                return;
            }

            try
            {
                Clipboard.SetText(HashResult);
                Status = "Хэш скопирован в буфер обмена";
            }
            catch (Exception ex)
            {
                Status = $"Ошибка при копировании: {ex.Message}";
                _logger.LogError($"Ошибка при копировании хэша: {ex}");
            }
        }

        private void UseHashForBruteForce()
        {
            if (string.IsNullOrEmpty(HashResult))
            {
                Status = "Сначала вычислите хэш";
                return;
            }

            TargetHash = HashResult;
            Status = "Хэш перенесен в поле целевого хэша";
        }

        private void RefreshDevices()
        {
            _ = RefreshDevicesAsync();
        }

        private async Task RefreshDevicesAsync()
        {
            try
            {
                var devices = await _deviceService.GetAvailableDevicesAsync();
                AvailableDevices = new ObservableCollection<Device>(devices);

                if (AvailableDevices.Count > 0)
                {
                    SelectedDevice = AvailableDevices[0];
                }

                LogText += $"Найдено устройств: {AvailableDevices.Count}\n";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при получении списка устройств: {ex.Message}");
                LogText += $"Ошибка при получении списка устройств: {ex.Message}\n";
            }
        }

        private void OnBruteForceProgressChanged(object? sender, BruteForceProgressEventArgs e)
        {
            // Для WPF необходимо использовать Dispatcher текущего потока, чтобы избежать ошибки STA
            if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null &&
                !System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => OnBruteForceProgressChanged(sender, e));
                return;
            }
            
            UpdateProgress(e);

            // Добавление результата производительности для графика
            if (e.TotalAttempts > 0 && e.ElapsedTime.TotalSeconds > 0)
            {
                var attemptsPerSecond = e.TotalAttempts / e.ElapsedTime.TotalSeconds;
                AttemptsPerSecond = attemptsPerSecond;

                // Добавляем новый результат каждые 5 секунд
                if (e.ElapsedTime.TotalSeconds % 5 < 1)
                {
                    int testNumber = PerformanceResults.Count + 1;
                    
                    var result = new PerformanceResult
                    {
                        DeviceId = SelectedDevice?.Id ?? "CPU-0",
                        DeviceName = SelectedDevice?.Name ?? "CPU",
                        ThreadCount = ThreadCount,
                        OperationsPerSecond = attemptsPerSecond,
                        Algorithm = SelectedAlgorithm,
                        Duration = e.ElapsedTime.TotalMilliseconds
                    };

                    var newResults = new List<PerformanceResult>(PerformanceResults)
                    {
                        result
                    };

                    PerformanceResults = newResults;

                    // Обновляем график
                    UpdateChartSeries();
                }
            }
        }

        private void UpdateProgress(BruteForceProgressEventArgs e)
        {
            // Убедимся, что мы в UI-потоке
            if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null &&
                !System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => UpdateProgress(e));
                return;
            }

            ProgressValue = e.Progress;
            CurrentAttempt = e.CurrentAttempt;
            ElapsedTime = e.ElapsedTime;
            Status = $"Выполнено {e.Progress}%";

            // Добавление в лог каждые 3 секунды
            if (e.ElapsedTime.TotalSeconds % 3 < 1)
            {
                LogText += $"[{e.ElapsedTime:hh\\:mm\\:ss}] Прогресс: {e.Progress}%, Текущая попытка: {e.CurrentAttempt}\n";
            }
        }

        private void UpdateChartSeries()
        {
            // Убедимся, что мы в UI-потоке
            if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null &&
                !System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                System.Windows.Application.Current.Dispatcher.Invoke(UpdateChartSeries);
                return;
            }

            if (_performanceResults.Count == 0)
            {
                // Если нет данных, создаем пустой график
                var emptySeries = new ColumnSeries
                {
                    Title = "Время выполнения (сек)",
                    Values = new ChartValues<double>()
                };
                ChartSeries = new SeriesCollection { emptySeries };
                ChartLabels = new string[] { "Нет данных" };
                return;
            }

            // Группируем результаты по алгоритму
            var algorithms = _performanceResults.Select(r => r.Algorithm).Distinct().ToList();

            var seriesCollection = new SeriesCollection();
            int maxResults = _performanceResults.Count;

            foreach (var algorithm in algorithms)
            {
                var series = new ColumnSeries
                {
                    Title = $"{algorithm}",
                    Values = new ChartValues<double>()
                };

                foreach (var result in _performanceResults.Where(r => r.Algorithm == algorithm))
                {
                    // Используем время в секундах
                    series.Values.Add(result.Duration / 1000.0); // преобразуем мс в секунды
                }

                seriesCollection.Add(series);
            }

            ChartSeries = seriesCollection;

            // Обновляем метки оси X
            var labels = new string[maxResults];
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i] = $"Тест {i + 1}";
            }

            ChartLabels = labels.Length > 0 ? labels : new string[] { "Нет данных" };
            
            // Обновляем форматтер для показа времени в секундах
            ChartFormatter = value => $"{value:F2} сек";
        }

        private void StopHashing()
        {
            _cancellationTokenSource.Cancel();
            Status = "Остановка хеширования...";
        }

        private void UpdateHashingChartSeries()
        {
            // Убедимся, что мы в UI-потоке
            if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null &&
                !System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                System.Windows.Application.Current.Dispatcher.Invoke(UpdateHashingChartSeries);
                return;
            }

            var series = new LineSeries
            {
                Title = "Время хеширования",
                Values = new ChartValues<double>(),
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 10
            };

            foreach (var result in _hashingPerformanceResults)
            {
                series.Values.Add(result.Duration);
            }

            var newSeries = new SeriesCollection { series };
            HashingChartSeries = newSeries;

            // Обновляем метки оси X
            var labels = new string[_hashingPerformanceResults.Count];
            for (int i = 0; i < _hashingPerformanceResults.Count; i++)
            {
                labels[i] = _hashingPerformanceResults[i].ThreadCount.ToString();
            }

            HashingChartLabels = labels;
        }

        private void ClearGraphsAndLogs()
        {
            // Убедимся, что мы в UI-потоке
            if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null &&
                !System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                System.Windows.Application.Current.Dispatcher.Invoke(ClearGraphsAndLogs);
                return;
            }

            // Очистка логов
            LogText = "Логи очищены\n";

            // Очистка графиков производительности хеширования
            HashingPerformanceResults = new List<HashPerformanceResult>();
            UpdateHashingChartSeries();

            // Очистка графиков брутфорса
            _performanceResults.Clear();
            BruteForcePerformanceResults = new List<BruteForcePerformanceResult>();
            UpdateBruteForceChartSeries();
            UpdateChartSeries();

            // Сброс прогресса
            ProgressValue = 0;
            CurrentAttempt = string.Empty;
            ElapsedTime = TimeSpan.Zero;
            Status = "Все графики и логи очищены";
            AttemptsPerSecond = 0;
        }

        private void AddBruteForcePerformanceResult(int threadCount, double durationMs)
        {
            // Проверяем, есть ли уже результат для этого количества потоков
            var results = new List<BruteForcePerformanceResult>(BruteForcePerformanceResults);
            var existingIndex = results.FindIndex(r => r.ThreadCount == threadCount);
            
            var result = new BruteForcePerformanceResult
            {
                ThreadCount = threadCount,
                Duration = durationMs,
                Algorithm = SelectedAlgorithm.ToString()
            };
            
            if (existingIndex >= 0)
            {
                results[existingIndex] = result;
            }
            else
            {
                results.Add(result);
            }
            
            BruteForcePerformanceResults = results.OrderBy(r => r.ThreadCount).ToList();
            UpdateBruteForceChartSeries();
        }

        private void UpdateBruteForceChartSeries()
        {
            // Убедимся, что мы в UI-потоке
            if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null &&
                !System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                System.Windows.Application.Current.Dispatcher.Invoke(UpdateBruteForceChartSeries);
                return;
            }

            if (_bruteForcePerformanceResults.Count == 0)
            {
                // Если нет данных, создаем пустой график
                var emptySeries = new LineSeries
                {
                    Title = "Время брутфорса (мс)",
                    Values = new ChartValues<double>(),
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 10
                };
                BruteForceChartSeries = new SeriesCollection { emptySeries };
                BruteForceChartLabels = new string[] { "Нет данных" };
                return;
            }

            var series = new LineSeries
            {
                Title = "Время брутфорса (мс)",
                Values = new ChartValues<double>(),
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 10
            };

            // Сортируем по количеству потоков для корректного отображения графика
            var sortedResults = _bruteForcePerformanceResults
                .OrderBy(r => r.ThreadCount)
                .ToList();

            foreach (var result in sortedResults)
            {
                series.Values.Add(result.Duration);
            }

            BruteForceChartSeries = new SeriesCollection { series };

            // Обновляем метки оси X - количество потоков
            var threadCounts = _bruteForcePerformanceResults
                .Select(r => r.ThreadCount)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            var labels = threadCounts.Select(t => t.ToString()).ToArray();
            BruteForceChartLabels = labels.Length > 0 ? labels : new string[] { "Нет данных" };

            // Обновляем форматтер для показа времени в миллисекундах
            BruteForceChartFormatter = value => $"{value:F2} мс";
        }

        // Добавляем метод для запуска тестирования производительности брутфорса с разным числом потоков
        public async void RunBruteForcePerformanceTest()
        {
            if (string.IsNullOrEmpty(TargetHash))
            {
                Status = "Введите хеш для подбора";
                LogText += "Ошибка: Введите хеш для подбора\n";
                return;
            }

            IsProcessing = true;
            Status = "Тестирование производительности брутфорса...";
            LogText += $"Начато тестирование производительности брутфорса\n";

            try
            {
                // Создаем новый токен отмены для каждой операции
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = new CancellationTokenSource();
                }

                var results = new List<BruteForcePerformanceResult>();
                var maxThreads = Math.Min(Environment.ProcessorCount, 8); // Ограничиваем количество потоков
                
                // Определяем небольшой хеш для тестирования
                var testHash = await _hashService.ComputeHashStringAsync("test", SelectedAlgorithm);
                
                // Сохраняем оригинальные настройки
                var originalHash = TargetHash;
                var originalMinLength = MinLength;
                var originalMaxLength = MaxLength;
                
                // Устанавливаем тестовые параметры - для быстрого тестирования
                TargetHash = testHash;
                MinLength = 1;
                MaxLength = 4;
                
                for (int threads = 1; threads <= maxThreads; threads++)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        break;

                    Status = $"Тестирование брутфорса ({threads} потоков)...";
                    LogText += $"Тестирование брутфорса: {threads} потоков\n";
                    
                    // Устанавливаем количество потоков для этого прохода
                    ThreadCount = threads;
                    
                    // Запускаем тестовый брутфорс с ограниченным временем
                    var startTime = DateTime.Now;
                    bool timeout = false;
                    var settings = new BruteForceSettings
                    {
                        TargetHash = TargetHash,
                        Algorithm = SelectedAlgorithm,
                        ThreadCount = ThreadCount,
                        Alphabet = Alphabet,
                        MinLength = MinLength,
                        MaxLength = MaxLength
                    };
                    
                    // Создаем таймаут для теста - не более 10 секунд на каждый тест
                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                    using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                        cts.Token, _cancellationTokenSource.Token))
                    {
                        try
                        {
                            await _bruteForceService.StartBruteForceAsync(settings, linkedCts.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            if (cts.IsCancellationRequested && !_cancellationTokenSource.IsCancellationRequested)
                            {
                                timeout = true;
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                    
                    var endTime = DateTime.Now;
                    var duration = (endTime - startTime).TotalMilliseconds;
                    
                    if (!timeout)
                    {
                        results.Add(new BruteForcePerformanceResult
                        {
                            ThreadCount = threads,
                            Duration = duration,
                            Algorithm = SelectedAlgorithm.ToString()
                        });
                    }
                    else
                    {
                        // Если был таймаут, записываем приблизительное значение
                        LogText += $"Тест для {threads} потоков прерван по таймауту\n";
                        results.Add(new BruteForcePerformanceResult
                        {
                            ThreadCount = threads,
                            Duration = 10000, // 10 секунд
                            Algorithm = SelectedAlgorithm.ToString()
                        });
                    }
                    
                    // Обновляем прогресс
                    ProgressValue = (int)((double)threads / maxThreads * 100);
                    
                    // Небольшая пауза между тестами
                    await Task.Delay(500, _cancellationTokenSource.Token);
                }
                
                // Восстанавливаем оригинальные настройки
                TargetHash = originalHash;
                MinLength = originalMinLength;
                MaxLength = originalMaxLength;
                
                // Сохраняем результаты и обновляем график
                BruteForcePerformanceResults = results;
                UpdateBruteForceChartSeries();
                
                Status = "Тестирование производительности брутфорса завершено";
                LogText += $"Тестирование производительности брутфорса завершено\n";
                
                // Определяем оптимальное количество потоков
                if (results.Count > 0)
                {
                    var optimalResult = results.OrderBy(r => r.Duration).First();
                    LogText += $"Лучший результат: {optimalResult.ThreadCount} потоков - {optimalResult.Duration:F2} мс\n";
                    LogText += $"Рекомендуемое количество потоков для брутфорса: {optimalResult.ThreadCount}\n";
                    
                    // Устанавливаем оптимальное количество потоков
                    ThreadCount = optimalResult.ThreadCount;
                }
            }
            catch (OperationCanceledException)
            {
                Status = "Операция отменена";
                LogText += "Тестирование отменено пользователем\n";
            }
            catch (Exception ex)
            {
                Status = $"Ошибка: {ex.Message}";
                LogText += $"Ошибка при тестировании: {ex.Message}\n";
                _logger.LogError($"Ошибка при тестировании производительности брутфорса: {ex}");
            }
            finally
            {
                IsProcessing = false;
                ProgressValue = 0;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Initialize()
        {
            RefreshDevicesAsync().Wait();
            LogText += "Приложение инициализировано.\n";
            LogText += $"Системная информация: {ProcessorInfo}, {ProcessorCount}, ОС: {OsInfo}\n";
            LogText += "Выберите настройки и запустите тест хеширования или брутфорс.\n";

            // Настройка форматтеров
            ChartFormatter = value => $"{value:F2} мс";
            HashingChartFormatter = value => value.ToString("N2");
            BruteForceChartFormatter = value => $"{value:F2} мс";

            // Инициализируем графики с пустыми данными
            UpdateChartSeries();
            UpdateHashingChartSeries();
            UpdateBruteForceChartSeries();
        }
    }

    public class HashPerformanceResult
    {
        public int ThreadCount { get; set; }
        public double Duration { get; set; }
        public double HashesPerSecond { get; set; }
    }

    public class BruteForcePerformanceResult
    {
        public int ThreadCount { get; set; }
        public double Duration { get; set; }
        public string Algorithm { get; set; } = string.Empty;
    }
}