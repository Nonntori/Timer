using System.Diagnostics;
using ShutdownTimer.Models;

namespace ShutdownTimer.Services;

public interface IShutdownService
{
    bool ScheduleShutdown(TimeSpan delay, ShutdownAction action);
    bool CancelShutdown();
    string GetShutdownCommand(TimeSpan delay, ShutdownAction action);
}

public class ShutdownService : IShutdownService
{
    public bool ScheduleShutdown(TimeSpan delay, ShutdownAction action)
    {
        try
        {
            var command = GetShutdownCommand(delay, action);
            var processInfo = new ProcessStartInfo
            {
                FileName = "shutdown.exe",
                Arguments = command,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(processInfo);
            if (process == null) return false;
            
            process.WaitForExit(5000);
            
            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                Debug.WriteLine($"Shutdown error: {error}");
                return false;
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ScheduleShutdown exception: {ex.Message}");
            return false;
        }
    }

    public bool CancelShutdown()
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "shutdown.exe",
                Arguments = "/a",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(processInfo);
            if (process == null) return false;
            
            process.WaitForExit(5000);
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CancelShutdown exception: {ex.Message}");
            return false;
        }
    }

    public string GetShutdownCommand(TimeSpan delay, ShutdownAction action)
    {
        int seconds = (int)delay.TotalSeconds;
        
        return action switch
        {
            ShutdownAction.Shutdown => $"/s /t {seconds} /c \"Таймер выключения: компьютер будет выключен\"",
            ShutdownAction.Restart => $"/r /t {seconds} /c \"Таймер выключения: компьютер будет перезапущен\"",
            ShutdownAction.LogOff => $"/l /t {seconds}",
            ShutdownAction.Sleep => "/h", // Гибернация для сна
            _ => $"/s /t {seconds}"
        };
    }
}
