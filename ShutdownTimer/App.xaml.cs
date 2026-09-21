using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ShutdownTimer.Services;
using ShutdownTimer.ViewModels;

namespace ShutdownTimer;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Сервисы
        services.AddSingleton<IShutdownService, ShutdownService>();
        services.AddSingleton<ITimerService, TimerService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IStartupService, StartupService>();
        services.AddSingleton<INotificationService, NotificationService>();
        
        // ViewModel
        services.AddTransient<MainViewModel>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Настраиваем тему приложения
        var settingsService = _serviceProvider.GetService<ISettingsService>();
        if (settingsService != null)
        {
            var settings = settingsService.LoadSettings();
            // Здесь можно применить тему
        }
    }
}
