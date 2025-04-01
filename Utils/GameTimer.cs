using System;
using System.Windows.Threading;

namespace AliciasSoftwareSim;

public class GameTimer
{
    private readonly DispatcherTimer _timer;
    private MainWindow.GameSpeed _speed = MainWindow.GameSpeed.Normal;

    public event Action OnTick;

    public GameTimer()
    {
        _timer = new DispatcherTimer();
        _timer.Tick += (_, _) => OnTick?.Invoke();
        SetSpeed(MainWindow.GameSpeed.Normal);
    }

    public void Start() => _timer.Start();
    public void Stop() => _timer.Stop();

    public void SetSpeed(MainWindow.GameSpeed speed)
    {
        if (speed == MainWindow.GameSpeed.Paused)
        {
            _timer.Stop();
            return;
        }

        _timer.Interval = speed switch
        {
            MainWindow.GameSpeed.Normal => TimeSpan.FromMilliseconds(1000),
            MainWindow.GameSpeed.Fast => TimeSpan.FromMilliseconds(500),
            MainWindow.GameSpeed.Ultra => TimeSpan.FromMilliseconds(250),
            _ => TimeSpan.FromMilliseconds(1000)
        };

        _timer.Start();
    }
}