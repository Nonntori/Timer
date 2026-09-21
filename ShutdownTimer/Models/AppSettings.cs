namespace ShutdownTimer.Models;

public enum ShutdownAction
{
    Shutdown = 0,
    Restart = 1,
    LogOff = 2,
    Sleep = 3
}

public enum TimerPreset
{
    None = 0,
    Minutes15 = 1,
    Minutes30 = 2,
    Hour1 = 3,
    Hour2 = 4,
    Custom = 5
}

public class AppSettings
{
    public bool AutoStart { get; set; } = false;
    public bool MinimizeToTray { get; set; } = true;
    public bool ShowNotifications { get; set; } = true;
    public int WarningMinutes { get; set; } = 5;
    public bool SoundEnabled { get; set; } = true;
    public ShutdownAction DefaultAction { get; set; } = ShutdownAction.Shutdown;
    public string Theme { get; set; } = "Dark"; // Dark, Light, System
    public string Language { get; set; } = "ru";
}
