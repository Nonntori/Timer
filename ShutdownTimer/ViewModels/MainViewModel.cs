using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShutdownTimer.Models;
using ShutdownTimer.Services;

namespace ShutdownTimer.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IShutdownService _shutdownService;
    private readonly ITimerService _timerService;
    private readonly INotificationService _notificationService;
    private readonly ISettingsService _settingsService;
    private readonly IStartupService _startupService;
    
    private bool _warningTriggered;
    private TimeSpan _lastWarningTime;

    [ObservableProperty]
    private string _remainingTime = "01:30:00";

    [ObservableProperty]
    private double _progress = 1.0;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private ShutdownAction _selectedAction = ShutdownAction.Shutdown;

    [ObservableProperty]
    private TimerPreset _selectedPreset = TimerPreset.Minutes30;

    [ObservableProperty]
    private bool _warningEnabled = true;

    [ObservableProperty]
    private int _warningMinutes = 5;

    [ObservableProperty]
    private string _statusText = "Компьютер выключится по истечении времени";

    [ObservableProperty]
    private string _startButtonText = "Запустить таймер";

    [ObservableProperty]
    private bool _canStart = true;

    [ObservableProperty]
    private Color _accentColor = Color.FromRgb(99, 110, 255);

    public MainViewModel(
        IShutdownService shutdownService,
        ITimerService timerService,
        INotificationService notificationService,
        ISettingsService settingsService,
        IStartupService startupService)
    {
        _shutdownService = shutdownService;
        _timerService = timerService;
        _notificationService = notificationService;
        _settingsService = settingsService;
        _startupService = startupService;

        _timerService.Tick += OnTimerTick;
        _timerService.Completed += OnTimerCompleted;
        
        LoadSettings();
        UpdateAccentColor();
    }

    private void LoadSettings()
    {
        var settings = _settingsService.LoadSettings();
        _selectedAction = settings.DefaultAction;
        _warningEnabled = settings.ShowNotifications;
        _warningMinutes = settings.WarningMinutes;
    }

    private void UpdateAccentColor()
    {
        AccentColor = _selectedAction switch
        {
            ShutdownAction.Shutdown => Color.FromRgb(99, 110, 255),
            ShutdownAction.Restart => Color.FromRgb(255, 140, 0),
            ShutdownAction.LogOff => Color.FromRgb(0, 198, 255),
            ShutdownAction.Sleep => Color.FromRgb(162, 117, 255),
            _ => Color.FromRgb(99, 110, 255)
        };
    }

    private void OnTimerTick(TimeSpan remaining)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            RemainingTime = remaining.ToString(@"hh\:mm\:ss");
            Progress = remaining.TotalSeconds / (_timerService.RemainingTime + (DateTimeOffset.Now - (_timerService.GetType().GetField("_endTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_timerService) as DateTimeOffset? ?? DateTimeOffset.Now))).TotalSeconds;
            
            // Проверка предупреждения
            if (_warningEnabled && !_warningTriggered && remaining.TotalMinutes <= _warningMinutes && remaining.TotalMinutes > 0)
            {
                _warningTriggered = true;
                _notificationService.ShowWarning(_selectedAction, _warningMinutes);
            }
        });
    }

    private void OnTimerCompleted()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            IsRunning = false;
            StartButtonText = "Запустить таймер";
            StatusText = "Действие выполняется...";
            _warningTriggered = false;
        });
    }

    [RelayCommand]
    private void SelectPreset(int presetValue)
    {
        if (IsRunning) return;
        
        SelectedPreset = (TimerPreset)presetValue;
        
        var duration = presetValue switch
        {
            1 => TimeSpan.FromMinutes(15),
            2 => TimeSpan.FromMinutes(30),
            3 => TimeSpan.FromHours(1),
            4 => TimeSpan.FromHours(2),
            _ => TimeSpan.FromMinutes(30)
        };
        
        RemainingTime = duration.ToString(@"hh\:mm\:ss");
        Progress = 1.0;
    }

    [RelayCommand]
    private void SetCustomTime()
    {
        if (IsRunning) return;
        
        // Здесь можно открыть диалог выбора времени
        SelectedPreset = TimerPreset.Custom;
    }

    [RelayCommand]
    private async Task StartAsync()
    {
        if (IsRunning)
        {
            CancelTimer();
            return;
        }

        var duration = SelectedPreset switch
        {
            TimerPreset.Minutes15 => TimeSpan.FromMinutes(15),
            TimerPreset.Minutes30 => TimeSpan.FromMinutes(30),
            TimerPreset.Hour1 => TimeSpan.FromHours(1),
            TimerPreset.Hour2 => TimeSpan.FromHours(2),
            TimerPreset.Custom => TimeSpan.FromMinutes(30),
            _ => TimeSpan.FromMinutes(30)
        };

        // Подтверждение
        var actionText = _selectedAction switch
        {
            ShutdownAction.Shutdown => "выключение компьютера",
            ShutdownAction.Restart => "перезагрузка компьютера",
            ShutdownAction.LogOff => "выход из системы",
            ShutdownAction.Sleep => "переход в спящий режим",
            _ => "действие"
        };

        var result = MessageBox.Show(
            $"Вы уверены, что хотите запланировать {actionText} через {duration.Minutes} мин.?",
            "Подтверждение",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        // Планируем системное выключение
        if (!_shutdownService.ScheduleShutdown(duration, _selectedAction))
        {
            MessageBox.Show("Не удалось запланировать выключение компьютера.", "Ошибка", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Запускаем таймер
        _timerService.StartTimer(duration, _selectedAction);
        
        IsRunning = true;
        StartButtonText = "Отменить таймер";
        StatusText = $"{GetActionDescription()} через";
        _warningTriggered = false;
        _lastWarningTime = TimeSpan.Zero;
        
        UpdateAccentColor();
        
        _notificationService.ShowNotification("Таймер запущен", 
            $"{GetActionDescription()} через {RemainingTime}");
    }

    private string GetActionDescription()
    {
        return _selectedAction switch
        {
            ShutdownAction.Shutdown => "Компьютер будет выключен",
            ShutdownAction.Restart => "Компьютер будет перезапущен",
            ShutdownAction.LogOff => "Будет выполнен выход из системы",
            ShutdownAction.Sleep => "Компьютер перейдет в спящий режим",
            _ => "Будет выполнено действие"
        };
    }

    [RelayCommand]
    private void CancelTimer()
    {
        _timerService.StopTimer();
        _shutdownService.CancelShutdown();
        
        IsRunning = false;
        StartButtonText = "Запустить таймер";
        StatusText = "Компьютер выключится по истечении времени";
        _warningTriggered = false;
        
        _notificationService.ShowNotification("Таймер отменён", "Запланированное действие отменено.");
    }

    [RelayCommand]
    private void OpenSettings()
    {
        // Открыть окно настроек
    }

    partial void OnSelectedActionChanged(ShutdownAction value)
    {
        UpdateAccentColor();
    }
}
