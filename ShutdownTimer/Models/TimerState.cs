using System.Windows.Media;

namespace ShutdownTimer.Models;

public class TimerState
{
    public TimeSpan RemainingTime { get; set; }
    public TimeSpan TotalTime { get; set; }
    public bool IsRunning { get; set; }
    public ShutdownAction SelectedAction { get; set; }
    public TimerPreset SelectedPreset { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public bool WarningShown { get; set; }
    public double Progress => TotalTime.TotalSeconds > 0 ? RemainingTime.TotalSeconds / TotalTime.TotalSeconds : 0;
    
    public string RemainingTimeString => RemainingTime.ToString(@"hh\:mm\:ss");
    
    public string ActionDescription => SelectedAction switch
    {
        ShutdownAction.Shutdown => "Выключение компьютера",
        ShutdownAction.Restart => "Перезагрузка компьютера",
        ShutdownAction.LogOff => "Выход из системы",
        ShutdownAction.Sleep => "Спящий режим",
        _ => "Неизвестное действие"
    };
    
    public Color AccentColor => SelectedAction switch
    {
        ShutdownAction.Shutdown => Color.FromRgb(99, 110, 255),
        ShutdownAction.Restart => Color.FromRgb(255, 140, 0),
        ShutdownAction.LogOff => Color.FromRgb(0, 198, 255),
        ShutdownAction.Sleep => Color.FromRgb(162, 117, 255),
        _ => Colors.Blue
    };
}
