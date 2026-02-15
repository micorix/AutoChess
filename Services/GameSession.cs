using System;
using System.Threading.Tasks;
using Rudzoft.ChessLib;
using Rudzoft.ChessLib.Factories;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;

namespace AutoChess.Services;

public sealed class GameSession : IDisposable
{
    private StockfishAdapter _stockfish = new();
    private string? _enginePath;

    public IGame Game { get; private set; } = default!;
    public bool IsWhiteTurn => Game.Pos.SideToMove == Player.White;

    public GameStatusManager Status { get; } = new();

    public event Action? BoardUpdated;

    public void SetEnginePath(string path)
    {
        _stockfish.Dispose();
        _stockfish = new StockfishAdapter();
        _enginePath = path;
    }

    public async Task StartNewGameAsync()
    {
        Game = GameFactory.Create();
        Game.NewGame();

        Status.NewGameStarted();
        BoardUpdated?.Invoke();

        await StartEngineAsync();
    }

    private async Task StartEngineAsync()
    {
        if (string.IsNullOrWhiteSpace(_enginePath) || !System.IO.File.Exists(_enginePath))
        {
            Status.EnginePathMissing(_enginePath, IsWhiteTurn);
            return;
        }

        try
        {
            if (!_stockfish.IsRunning)
            {
                await _stockfish.StartAsync(_enginePath);
                Status.EngineWaiting(IsWhiteTurn);
            }

            await _stockfish.NewGameAsync();
            Status.EngineReady(IsWhiteTurn);
        }
        catch (Exception ex)
        {
            Status.EngineError(ex, IsWhiteTurn);
        }
    }

    public bool TryMakeMove(Square from, Square to)
    {
        Status.MoveAttempt(from, to, IsWhiteTurn);

        var legalMoves = Game.Pos.GenerateMoves();
        Status.AvailableMoves(legalMoves.Length, IsWhiteTurn);

        var move = FindMove(legalMoves, from, to);

        if (move.IsNullMove() && IsPawnPromotion(from, to))
        {
            move = FindMove(legalMoves, from, to, PieceTypes.Queen);
        }

        if (move.IsNullMove())
        {
            Status.MoveIllegal(IsWhiteTurn);
            return false;
        }

        var wasWhiteTurn = IsWhiteTurn;
        Game.Pos.MakeMove(move, Game.Pos.State);
        Status.MoveExecuted(from, to, wasWhiteTurn);
        BoardUpdated?.Invoke();

        if (IsGameOver())
        {
            Status.GameOver(IsWhiteTurn);
        }

        return true;
    }

    public bool IsGameOver()
    {
        var moves = Game.Pos.GenerateMoves();
        return moves.Length == 0;
    }

    public async Task MakeEngineMoveAsync()
    {
        if (!_stockfish.IsRunning)
        {
            Status.EngineNotRunning(IsWhiteTurn);
            return;
        }

        if (IsGameOver())
        {
            Status.GameOver(IsWhiteTurn);
            return;
        }

        Status.EngineThinking(IsWhiteTurn);

        try
        {
            var fen = Game.Pos.FenNotation;
            var bestMove = await _stockfish.GetBestMoveAsync(fen, 1000);

            if (string.IsNullOrEmpty(bestMove))
            {
                Status.EngineNoMove(IsWhiteTurn);
                return;
            }

            ApplyEngineMove(bestMove);
        }
        catch (Exception ex)
        {
            Status.EngineError(ex, IsWhiteTurn);
        }
    }

    private void ApplyEngineMove(string uciMove)
    {
        if (uciMove.Length < 4)
        {
            Status.EngineMoveInvalid(uciMove, IsWhiteTurn);
            return;
        }

        var fromSq = new Square(uciMove.Substring(0, 2));
        var toSq = new Square(uciMove.Substring(2, 2));

        PieceTypes promo = PieceTypes.NoPieceType;
        if (uciMove.Length > 4)
        {
            promo = uciMove[4] switch
            {
                'q' => PieceTypes.Queen,
                'r' => PieceTypes.Rook,
                'b' => PieceTypes.Bishop,
                'n' => PieceTypes.Knight,
                _ => PieceTypes.Queen,
            };
        }

        var legalMoves = Game.Pos.GenerateMoves();
        var move = FindMove(legalMoves, fromSq, toSq, promo);

        if (!move.IsNullMove())
        {
            var wasWhiteTurn = IsWhiteTurn;
            Game.Pos.MakeMove(move, Game.Pos.State);
            Status.EngineMoveExecuted(uciMove, wasWhiteTurn);
            BoardUpdated?.Invoke();

            if (IsGameOver())
            {
                Status.GameOver(IsWhiteTurn);
            }
        }
        else
        {
            Status.EngineMoveInvalid(uciMove, IsWhiteTurn);
        }
    }

    private static Move FindMove(MoveList moves, Square from, Square to, PieceTypes promo = PieceTypes.NoPieceType)
    {
        foreach (var m in moves)
        {
            if (m.Move.FromSquare() != from || m.Move.ToSquare() != to)
            {
                continue;
            }

            if (promo != PieceTypes.NoPieceType && m.Move.PromotedPieceType() != promo)
            {
                continue;
            }

            return m.Move;
        }

        return Move.EmptyMove;
    }

    private bool IsPawnPromotion(Square from, Square to)
    {
        var piece = Game.Pos.GetPiece(from);
        if (piece.Type() != PieceTypes.Pawn)
        {
            return false;
        }

        return (Game.Pos.SideToMove == Player.White && to.Rank == Rank.Rank8)
            || (Game.Pos.SideToMove == Player.Black && to.Rank == Rank.Rank1);
    }

    public void Dispose()
    {
        _stockfish.Dispose();
    }
}
