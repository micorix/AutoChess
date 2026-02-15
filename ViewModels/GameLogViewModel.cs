using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace AutoChess.ViewModels;

public class GameLogViewModel
{
    private readonly Dispatcher _dispatcher = Application.Current.Dispatcher;

    public ObservableCollection<string> Entries { get; } = new();

    public void AddEntry(string message)
    {
        var formatted = $"[{DateTime.Now:HH:mm:ss}] {message}";

        if (_dispatcher.CheckAccess())
        {
            Entries.Add(formatted);
        }
        else
        {
            _dispatcher.BeginInvoke(() => Entries.Add(formatted));
        }
    }

    public void Clear()
    {
        if (_dispatcher.CheckAccess())
        {
            Entries.Clear();
        }
        else
        {
            _dispatcher.BeginInvoke(() => Entries.Clear());
        }
    }
}
