using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using multi_threaded_hashing.Services;
using multi_threaded_hashing.Services.Interfaces;
using multi_threaded_hashing.ViewModels;
using multi_threaded_hashing.Views;
using System;
using System.Windows.Threading;

namespace multi_threaded_hashing;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly ServiceProvider _serviceProvider;

    public App()
    {
        // Добавляем глобальную обработку исключений
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        DispatcherUnhandledException += App_DispatcherUnhandledException;

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // Показываем сообщение об ошибке
        MessageBox.Show($"Возникло необработанное исключение: {e.Exception.Message}\n\nStack trace: {e.Exception.StackTrace}", 
            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        
        // Помечаем исключение как обработанное, чтобы приложение не закрылось
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            MessageBox.Show($"Критическая ошибка: {ex.Message}\n\nStack trace: {ex.StackTrace}", 
                "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ConfigureServices(ServiceCollection services)
    {
        // Регистрируем логгер
        services.AddSingleton<ILogger, ConsoleLogger>();
        
        // Регистрируем сервисы
        services.AddSingleton<IDeviceService, DeviceService>();
        services.AddSingleton<IHashService, HashService>();
        services.AddSingleton<IBruteForceService>(provider => 
            new BruteForceService(provider.GetRequiredService<IHashService>()));
        
        // Регистрируем ViewModel
        services.AddSingleton<MainViewModel>(provider => 
            new MainViewModel(
                provider.GetRequiredService<IBruteForceService>(),
                provider.GetRequiredService<IHashService>(),
                provider.GetRequiredService<IDeviceService>(),
                provider.GetRequiredService<ILogger>()
            ));
        
        // Регистрируем MainWindow
        services.AddSingleton<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            base.OnStartup(e);
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при запуске приложения: {ex.Message}\n\nStack trace: {ex.StackTrace}", 
                "Ошибка запуска", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        _serviceProvider.Dispose();
    }
}

