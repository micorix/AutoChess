using System;
using System.Windows;
using AutoChess.Services;
using AutoChess.ViewModels;
using Microsoft.Win32;
using Rudzoft.ChessLib.Types;

namespace AutoChess;

public partial class MainWindow : Window, IDisposable
{
    private GameSession? _gameSession;
    private readonly ToastViewModel _toastVM = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _toastVM;
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _gameSession = new GameSession();

            ControlsView.GameStatus = _gameSession.Status.StatusVM;
            ControlsView.GameLog = _gameSession.Status.LogVM;

            _gameSession.BoardUpdated += OnBoardUpdated;

            BoardView.MoveRequested += OnUserMoveRequested;
            ControlsView.NewGameRequested += OnNewGameRequested;

            await _gameSession.StartNewGameAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        Dispose();
    }

    public void Dispose()
    {
        if (_gameSession != null)
        {
            _gameSession.BoardUpdated -= OnBoardUpdated;
            _gameSession.Dispose();
            _gameSession = null;
        }
        GC.SuppressFinalize(this);
    }


    private void SelectEngine_Click(object sender, RoutedEventArgs e)
    {
        if (_gameSession == null)
        {
            return;
        }

        var dlg = new OpenFileDialog
        {
            Title = GameStatusManager.UI.EngineFileDialogTitle,
            Filter = GameStatusManager.UI.EngineFileDialogFilter,
            CheckFileExists = true,
        };

        if (dlg.ShowDialog(this) == true)
        {
            _gameSession.SetEnginePath(dlg.FileName);
            _gameSession.Status.EngineChanged(dlg.FileName);
            _toastVM.ShowToast(GameStatusManager.UI.EngineChangedToast, 3000);
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e) => Close();

    private void OnBoardUpdated()
    {
        Dispatcher.BeginInvoke(() =>
        {
            if (_gameSession != null)
            {
                BoardView.Render(_gameSession.Game);
            }
        });
    }

    private async void OnNewGameRequested(object? sender, EventArgs e)
    {
        try
        {
            if (_gameSession != null)
            {
                await _gameSession.StartNewGameAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void OnUserMoveRequested(Square from, Square to)
    {
        try
        {
            if (_gameSession == null || !_gameSession.IsWhiteTurn || _gameSession.IsGameOver())
            {
                return;
            }

            bool success = _gameSession.TryMakeMove(from, to);
            if (!success)
            {
                _toastVM.ShowToast(GameStatusManager.UI.InvalidMoveToast);
                return;
            }

            await _gameSession.MakeEngineMoveAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
