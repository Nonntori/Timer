using System.IO;
using Newtonsoft.Json;
using ShutdownTimer.Models;

namespace ShutdownTimer.Services;

public interface ISettingsService
{
    AppSettings LoadSettings();
    void SaveSettings(AppSettings settings);
    string SettingsPath { get; }
}

public class SettingsService : ISettingsService
{
    private readonly string _settingsDir;
    public string SettingsPath => Path.Combine(_settingsDir, "settings.json");

    public SettingsService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _settingsDir = Path.Combine(appData, "ShutdownTimer");
        
        if (!Directory.Exists(_settingsDir))
        {
            Directory.CreateDirectory(_settingsDir);
        }
    }

    public AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var settings = JsonConvert.DeserializeObject<AppSettings>(json);
                return settings ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadSettings error: {ex.Message}");
        }
        
        return new AppSettings();
    }

    public void SaveSettings(AppSettings settings)
    {
        try
        {
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(SettingsPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SaveSettings error: {ex.Message}");
        }
    }
}
