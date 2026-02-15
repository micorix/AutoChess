using AutoChess.ViewModels;
using Rudzoft.ChessLib.Types;
using System;

namespace AutoChess.Services;

public sealed class GameStatusManager
{
    public GameStatusViewModel StatusVM { get; } = new()
    {
        TurnText = UI.DefaultTurn,
        StateText = UI.DefaultState,
    };

    public GameLogViewModel LogVM { get; } = new();


    public void NewGameStarted()
    {
        LogVM.Clear();
        StatusVM.TurnText = TurnText(true);
        StatusVM.StateText = "Oczekiwanie...";
        LogVM.AddEntry("Nowa gra rozpoczęta");
    }

    public void EnginePathMissing(string? path, bool isWhiteTurn)
    {
        StatusVM.TurnText = TurnText(isWhiteTurn);
        StatusVM.StateText = "Oczekiwanie...";
        LogVM.AddEntry($"Brak Stockfisha: {path}");
    }

    public void EngineWaiting(bool isWhiteTurn)
    {
        StatusVM.TurnText = TurnText(isWhiteTurn);
        StatusVM.StateText = "Oczekiwanie...";
        LogVM.AddEntry("Oczekiwanie na Stockfisha...");
    }

    public void EngineReady(bool isWhiteTurn)
    {
        StatusVM.TurnText = TurnText(isWhiteTurn);
        StatusVM.StateText = TurnText(isWhiteTurn);
        LogVM.AddEntry("Stockfish gotowy");
    }

    public void EngineChanged(string path)
    {
        StatusVM.StateText = "Oczekiwanie...";
        LogVM.AddEntry($"Zmieniono plik exe Stockfisha: {path}");
    }

    public void EngineError(Exception ex, bool isWhiteTurn)
    {
        StatusVM.TurnText = TurnText(isWhiteTurn);
        StatusVM.StateText = "Błąd silnika";
        LogVM.AddEntry($"Błąd: {ex.Message}");
    }

    public void EngineNotRunning(bool isWhiteTurn)
    {
        StatusVM.TurnText = TurnText(isWhiteTurn);
        StatusVM.StateText = "Błąd Stockfisha";
        LogVM.AddEntry("Stockfish nie jest uruchomiony.");
    }

    public void EngineThinking(bool isWhiteTurn)
    {
        StatusVM.TurnText = TurnText(isWhiteTurn);
        StatusVM.StateText = "Myślenie...";
        LogVM.AddEntry("Stockfish myśli...");
    }

    public void EngineNoMove(bool isWhiteTurn)
    {
        StatusVM.TurnText = TurnText(isWhiteTurn);
        StatusVM.StateText = "Błąd Stockfisha";
        LogVM.AddEntry("Stockfish nie zwrócił ruchu.");
    }

    public void EngineMoveExecuted(string uciMove, bool isWhiteTurn)
    {
        StatusVM.TurnText = TurnText(isWhiteTurn);
        StatusVM.StateText = TurnText(isWhiteTurn);
        LogVM.AddEntry($"Stockfish: {uciMove}");
    }

    public void EngineMoveInvalid(string uciMove, bool isWhiteTurn)
    {
        StatusVM.TurnText = TurnText(isWhiteTurn);
        StatusVM.StateText = "Błąd Stockfisha";
        LogVM.AddEntry($"Nieprawidłowy ruch: {uciMove}");
    }

    public void MoveAttempt(Square from, Square to, bool isWhiteTurn)
    {
        StatusVM.TurnText = TurnText(isWhiteTurn);
        StatusVM.StateText = TurnText(isWhiteTurn);
        LogVM.AddEntry($"Próba ruchu: {from}->{to}");
    }

    public void AvailableMoves(int count, bool isWhiteTurn)
    {
        StatusVM.TurnText = TurnText(isWhiteTurn);
        StatusVM.StateText = TurnText(isWhiteTurn);
        LogVM.AddEntry($"Dostępne ruchy: {count}");
    }

    public void MoveIllegal(bool isWhiteTurn)
    {
        StatusVM.TurnText = TurnText(isWhiteTurn);
        StatusVM.StateText = "Zły ruch";
        LogVM.AddEntry("Ruch niedozwolony.");
    }

    public void MoveExecuted(Square from, Square to, bool isWhiteTurn)
    {
        StatusVM.TurnText = TurnText(isWhiteTurn);
        StatusVM.StateText = TurnText(isWhiteTurn);
        LogVM.AddEntry($"Ruch: {from}->{to}");
    }

    public void GameOver(bool isWhiteTurn)
    {
        var winner = isWhiteTurn ? "Czarne" : "Białe";
        StatusVM.TurnText = "Koniec gry";
        StatusVM.StateText = $"{winner} wygrały!";
        LogVM.AddEntry($"Koniec gry - {winner} wygrały!");
    }

    private static string TurnText(bool isWhiteTurn)
        => isWhiteTurn ? "Tura białych" : "Tura czarnych";

    public static class UI
    {
        public const string AppTitle = "AutoChess";
        public const string ButtonNewGame = "Nowa gra";
        public const string GroupHistory = "Historia";

        public const string MenuFile = "Plik";
        public const string MenuSelectEngine = "Wybierz plik exe Stockfisha...";
        public const string MenuClose = "Zamknij";

        public const string GroupGameStatus = "Status gry";
        public const string GroupControls = "Sterowanie";
        public const string DefaultTurn = "Tura białych";
        public const string DefaultState = "Oczekiwanie...";

        public const string EngineFileDialogTitle = "Wybierz plik exe Stockfisha";
        public const string EngineFileDialogFilter = "Pliki wykonywalne (*.exe)|*.exe";
        public const string EngineChangedToast = "Stockfish został załadowany.";
        public const string InvalidMoveToast = "Nieprawidłowy ruch!";
    }

    public static class Exceptions
    {
        public const string EngineNotRunning = "Stockfish nie działa.";
        public const string QueryStillPending = "Poprzednie zapytanie jeszcze trwa.";
        public static string EngineNotFound(string path) => $"Nie znaleziono: {path}";
    }
}
