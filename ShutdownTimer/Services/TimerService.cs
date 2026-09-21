using ShutdownTimer.Models;

namespace ShutdownTimer.Services;

public interface ITimerService
{
    void StartTimer(TimeSpan duration, ShutdownAction action);
    void StopTimer();
    void PauseTimer();
    void ResumeTimer();
    bool IsRunning { get; }
    TimeSpan RemainingTime { get; }
    event Action<TimeSpan>? Tick;
    event Action? Completed;
}

public class TimerService : ITimerService
{
    private System.Timers.Timer? _timer;
    private DateTimeOffset? _endTime;
    private TimeSpan _totalTime;
    private bool _isPaused;
    
    public bool IsRunning => _timer?.Enabled == true && !_isPaused;
    public TimeSpan RemainingTime => _endTime.HasValue 
        ? _endTime.Value - DateTimeOffset.Now 
        : TimeSpan.Zero;

    public event Action<TimeSpan>? Tick;
    public event Action? Completed;

    public void StartTimer(TimeSpan duration, ShutdownAction action)
    {
        StopTimer();
        
        _totalTime = duration;
        _endTime = DateTimeOffset.Now + duration;
        _isPaused = false;
        
        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
        _timer.Enabled = true;
    }

    private void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (!_endTime.HasValue) return;
        
        var remaining = _endTime.Value - DateTimeOffset.Now;
        
        if (remaining.TotalSeconds <= 0)
        {
            StopTimer();
            Completed?.Invoke();
            return;
        }
        
        Tick?.Invoke(remaining);
    }

    public void StopTimer()
    {
        if (_timer != null)
        {
            _timer.Elapsed -= OnTimerElapsed;
            _timer.Stop();
            _timer.Dispose();
            _timer = null;
        }
        
        _endTime = null;
        _isPaused = false;
    }

    public void PauseTimer()
    {
        if (_timer == null || _isPaused || !_endTime.HasValue) return;
        
        _isPaused = true;
        _timer.Stop();
    }

    public void ResumeTimer()
    {
        if (_timer == null || !_isPaused || !_endTime.HasValue) return;
        
        _isPaused = false;
        _timer.Start();
    }
}
