using Microsoft.Win32;
using System.Diagnostics;

namespace ShutdownTimer.Services;

public interface IStartupService
{
    bool IsAutoStartEnabled();
    void SetAutoStart(bool enabled);
}

public class StartupService : IStartupService
{
    private const string RegistryKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
    private const string AppName = "ShutdownTimer";

    public bool IsAutoStartEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, false);
            if (key == null) return false;
            
            var value = key.GetValue(AppName);
            return value != null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"IsAutoStartEnabled error: {ex.Message}");
            return false;
        }
    }

    public void SetAutoStart(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
            if (key == null) return;
            
            if (enabled)
            {
                var exePath = Process.GetCurrentProcess().MainModule?.FileName ?? 
                              System.Reflection.Assembly.GetExecutingAssembly().Location;
                key.SetValue(AppName, $"\"{exePath}\"");
            }
            else
            {
                key.DeleteValue(AppName, false);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SetAutoStart error: {ex.Message}");
        }
    }
}
