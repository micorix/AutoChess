using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using AutoChess.ViewModels;

namespace AutoChess.Views;

public partial class GameControlsView : UserControl
{
    public event EventHandler? NewGameRequested;

    public static readonly DependencyProperty GameStatusProperty =
        DependencyProperty.Register(
            nameof(GameStatus),
            typeof(GameStatusViewModel),
            typeof(GameControlsView),
            new PropertyMetadata(null));

    public GameStatusViewModel? GameStatus
    {
        get => (GameStatusViewModel?)GetValue(GameStatusProperty);
        set => SetValue(GameStatusProperty, value);
    }

    public static readonly DependencyProperty GameLogProperty =
        DependencyProperty.Register(
            nameof(GameLog),
            typeof(GameLogViewModel),
            typeof(GameControlsView),
            new PropertyMetadata(null, OnGameLogChanged));

    public GameLogViewModel? GameLog
    {
        get => (GameLogViewModel?)GetValue(GameLogProperty);
        set => SetValue(GameLogProperty, value);
    }

    public GameControlsView()
    {
        InitializeComponent();
    }

    private static void OnGameLogChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not GameControlsView view)
        {
            return;
        }

        if (e.OldValue is GameLogViewModel oldVm)
        {
            oldVm.Entries.CollectionChanged -= view.OnLogEntriesChanged;
        }

        if (e.NewValue is GameLogViewModel newVm)
        {
            newVm.Entries.CollectionChanged += view.OnLogEntriesChanged;
        }
    }

    private void OnLogEntriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Dispatcher.Invoke(() => LogScrollViewer.ScrollToBottom());
    }

    private void NewGame_Click(object sender, RoutedEventArgs e)
    {
        NewGameRequested?.Invoke(this, EventArgs.Empty);
    }
}
