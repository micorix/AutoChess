using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AutoChess.ViewModels;

public class GameStatusViewModel : INotifyPropertyChanged
{
    private string _turnText = string.Empty;
    private string _stateText = string.Empty;

    public string TurnText
    {
        get => _turnText;
        set { if (_turnText != value) { _turnText = value; OnPropertyChanged(); } }
    }

    public string StateText
    {
        get => _stateText;
        set { if (_stateText != value) { _stateText = value; OnPropertyChanged(); } }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
