using System;
using System.Windows;
using System.Windows.Threading;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;
using BruteForceAnalyzer.Services;
using BruteForceAnalyzer.Services.Interfaces;
using BruteForceAnalyzer.ViewModels;
using LiveCharts;
using LiveCharts.Wpf;

namespace BruteForceAnalyzer.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IBruteForceService _bruteForceService;
        private readonly IHashService _hashService;
        private readonly IDeviceService _deviceService;
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Stopwatch _stopwatch;
        private readonly DispatcherTimer _timer;
        private string _foundPassword;
        private readonly MainViewModel _viewModel;

        public MainWindow(
            IBruteForceService bruteForceService,
            IHashService hashService,
            IDeviceService deviceService,
            ILogger logger,
            MainViewModel viewModel)
        {
            InitializeComponent();

            _bruteForceService = bruteForceService;
            _hashService = hashService;
            _deviceService = deviceService;
            _logger = logger;
            _cancellationTokenSource = new CancellationTokenSource();
            _stopwatch = new Stopwatch();
            _timer = new DispatcherTimer();
            _foundPassword = string.Empty;
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            DataContext = _viewModel;
            
            // Инициализация графика
            PerformanceChart.Series = new SeriesCollection();
            PerformanceChart.AxisX = new AxesCollection 
            { 
                new Axis 
                { 
                    Title = "Тест", 
                    Labels = new string[] { } 
                } 
            };
            PerformanceChart.AxisY = new AxesCollection 
            { 
                new Axis 
                { 
                    Title = "Хешей в секунду", 
                    LabelFormatter = value => value.ToString("N0") 
                } 
            };

            // Подписка на событие обновления данных производительности
            _viewModel.PerformanceResultsUpdated += (sender, args) => UpdateChart();

            // Инициализация начальных данных
            _viewModel.Initialize();
        }

        private void UpdateChart()
        {
            // Выполняем в потоке UI
            Dispatcher.Invoke(() =>
            {
                if (PerformanceChart?.Series == null || _viewModel.PerformanceResults.Count == 0)
                    return;

                var results = _viewModel.PerformanceResults;
                
                // Очищаем существующие серии
                PerformanceChart.Series.Clear();
                
                // Создаем новую серию для скорости хеширования
                var hashRateSeries = new ColumnSeries
                {
                    Title = "Хешей в секунду",
                    Values = new ChartValues<double>()
                };

                // Добавляем значения
                foreach (var result in results)
                {
                    hashRateSeries.Values.Add(result.HashesPerSecond);
                }

                // Добавляем серию в коллекцию
                PerformanceChart.Series.Add(hashRateSeries);

                // Обновляем метки оси X
                var labels = new string[results.Count];
                for (int i = 0; i < results.Count; i++)
                {
                    labels[i] = $"Тест {i + 1}";
                }

                if (PerformanceChart.AxisX != null && PerformanceChart.AxisX.Count > 0)
                {
                    PerformanceChart.AxisX[0].Labels = labels;
                }
            });
        }
    }
} 