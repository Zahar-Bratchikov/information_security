using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using BruteForceAnalyzer.Services;
using BruteForceAnalyzer.Services.Interfaces;
using BruteForceAnalyzer.ViewModels;
using BruteForceAnalyzer.Views;

namespace BruteForceAnalyzer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly ServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(ServiceCollection services)
    {
        services.AddSingleton<ILogger, ConsoleLogger>();
        services.AddSingleton<IDeviceService, DeviceService>();
        services.AddSingleton<IHashService, HashService>();
        services.AddSingleton<IBruteForceService, BruteForceService>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        _serviceProvider.Dispose();
    }
}

