using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace AutoChess.ViewModels;

public class ToastViewModel : INotifyPropertyChanged
{
    private string _toastMessage = string.Empty;
    private bool _isToastVisible;
    private DispatcherTimer? _toastTimer;
    private const int DefaultToastDurationMs = 2000;

    public string ToastMessage
    {
        get => _toastMessage;
        set { _toastMessage = value; OnPropertyChanged(); }
    }

    public bool IsToastVisible
    {
        get => _isToastVisible;
        set { _isToastVisible = value; OnPropertyChanged(); }
    }

    public void ShowToast(string message, int durationMs = DefaultToastDurationMs)
    {
        ToastMessage = message;
        IsToastVisible = true;

        if (_toastTimer != null)
        {
            _toastTimer.Stop();
            _toastTimer.Tick -= OnToastTimerTick;
        }

        _toastTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(durationMs) };
        _toastTimer.Tick += OnToastTimerTick;
        _toastTimer.Start();
    }

    private void OnToastTimerTick(object? sender, EventArgs e)
    {
        if (_toastTimer != null)
        {
            _toastTimer.Stop();
            _toastTimer.Tick -= OnToastTimerTick;
        }

        IsToastVisible = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
