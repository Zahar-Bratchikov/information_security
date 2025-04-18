using multi_threaded_hashing.ViewModels;
using System.Windows;

namespace multi_threaded_hashing.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel)
        {
            try
            {
                InitializeComponent();

                _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
                DataContext = _viewModel;

                // Подписка на событие обновления данных производительности
                _viewModel.PerformanceResultsUpdated += (sender, args) => UpdateChart();

                // Инициализация начальных данных
                _viewModel.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации окна: {ex.Message}\n\nStack trace: {ex.StackTrace}",
                    "Ошибка инициализации", MessageBoxButton.OK, MessageBoxImage.Error);
                throw; // Пробрасываем исключение дальше для отладки
            }
        }

        private void UpdateChart()
        {
            // Это может вызывать конфликт с привязками данных в XAML
            // Оставляем этот метод пустым, поскольку обновление графиков
            // должно происходить автоматически через привязки данных
        }
    }
}