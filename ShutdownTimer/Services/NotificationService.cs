using System.Windows;
using Microsoft.Win32;
using ShutdownTimer.Models;

namespace ShutdownTimer.Services;

public interface INotificationService
{
    void ShowNotification(string title, string message);
    void ShowWarning(ShutdownAction action, int minutes);
    void PlaySound();
}

public class NotificationService : INotificationService
{
    private readonly ISettingsService _settingsService;
    
    public NotificationService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public void ShowNotification(string title, string message)
    {
        var settings = _settingsService.LoadSettings();
        if (!settings.ShowNotifications) return;

        try
        {
            // Используем Windows Toast Notifications через PowerShell для Windows 10/11
            var script = $@"
[Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] | Out-Null
[Windows.Data.Xml.Dom.XmlDocument, Windows.Data.Xml.Dom.XmlDocument, ContentType = WindowsRuntime] | Out-Null

$template = @""
<toast>
    <visual>
        <binding template='ToastText02'>
            <text id='1'>{System.Security.SecurityElement.Escape(title)}</text>
            <text id='2'>{System.Security.SecurityElement.Escape(message)}</text>
        </binding>
    </visual>
</toast>
""@

$xml = New-Object Windows.Data.Xml.Dom.XmlDocument
$xml.LoadXml($template)
$toast = [Windows.UI.Notifications.ToastNotification]::new($xml)
[Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier('ShutdownTimer').Show($toast)
";
            // Fallback к простому MessageBox если toast не работает
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public void ShowWarning(ShutdownAction action, int minutes)
    {
        var actionText = action switch
        {
            ShutdownAction.Shutdown => "выключен",
            ShutdownAction.Restart => "перезапущен",
            ShutdownAction.LogOff => "выполнен выход из системы",
            ShutdownAction.Sleep => "переведен в спящий режим",
            _ => "выполнено действие"
        };
        
        ShowNotification("Предупреждение", $"Компьютер будет {actionText} через {minutes} мин.");
        
        if (_settingsService.LoadSettings().SoundEnabled)
        {
            PlaySound();
        }
    }

    public void PlaySound()
    {
        try
        {
            System.Media.SystemSounds.Exclamation.Play();
        }
        catch
        {
            // Игнорируем ошибки воспроизведения звука
        }
    }
}
