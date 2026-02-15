using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using AutoChess.ViewModels;
using Rudzoft.ChessLib;
using Rudzoft.ChessLib.Types;

namespace AutoChess.Views;

public partial class ChessBoardView : UserControl
{
    public event Action<Square, Square>? MoveRequested;

    private SquareViewModel? _selectedSquare;
    private IGame? _currentGame;

    public ObservableCollection<SquareViewModel> Squares { get; } = new();

    public ChessBoardView()
    {
        InitializeComponent();
        BuildSquares();
    }

    private void BuildSquares()
    {
        Squares.Clear();

        for (int rank = 7; rank >= 0; rank--)
        {
            for (int file = 0; file < 8; file++)
            {
                var square = new Square(rank, file);
                var squareVm = new SquareViewModel(square, rank, file);
                Squares.Add(squareVm);
            }
        }
    }

    public void Render(IGame game)
    {
        _currentGame = game;
        var pos = game.Pos;

        foreach (var squareVm in Squares)
        {
            var piece = pos.GetPiece(squareVm.Square);
            squareVm.Piece = piece;
            squareVm.IsSelected = squareVm == _selectedSquare;
        }
    }

    private void Cell_Click(object sender, MouseButtonEventArgs e)
    {
        if (_currentGame == null)
        {
            return;
        }

        if (sender is not Border border)
        {
            return;
        }

        if (border.DataContext is not SquareViewModel clickedSquareVm)
        {
            return;
        }

        var sq = clickedSquareVm.Square;

        if (_selectedSquare == null)
        {
            var piece = _currentGame.Pos.GetPiece(sq);

            if (piece != Piece.EmptyPiece && piece.ColorOf() == _currentGame.Pos.SideToMove)
            {
                SelectSquare(clickedSquareVm);
            }
            return;
        }

        if (clickedSquareVm == _selectedSquare)
        {
            SelectSquare(null);
            return;
        }

        var clickedPiece = _currentGame.Pos.GetPiece(sq);
        if (clickedPiece != Piece.EmptyPiece && clickedPiece.ColorOf() == _currentGame.Pos.SideToMove)
        {
            SelectSquare(clickedSquareVm);
            return;
        }

        var from = _selectedSquare.Square;
        SelectSquare(null);
        MoveRequested?.Invoke(from, sq);
    }

    private void SelectSquare(SquareViewModel? squareVm)
    {
        if (_selectedSquare == squareVm)
        {
            return;
        }

        if (_selectedSquare != null)
        {
            _selectedSquare.IsSelected = false;
        }

        _selectedSquare = squareVm;

        if (_selectedSquare != null)
        {
            _selectedSquare.IsSelected = true;
        }
    }
}
