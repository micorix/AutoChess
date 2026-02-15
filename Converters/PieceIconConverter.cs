using System;
using System.Globalization;
using System.Windows.Data;
using Rudzoft.ChessLib.Types;

namespace AutoChess.Converters;

public sealed class PieceIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Piece piece || piece == Piece.EmptyPiece)
        {
            return string.Empty;
        }

        var isWhite = piece.ColorOf() == Player.White;

        return (piece.Type(), isWhite) switch
        {
            (PieceTypes.King, true) => "♔",
            (PieceTypes.King, false) => "♚",
            (PieceTypes.Queen, true) => "♕",
            (PieceTypes.Queen, false) => "♛",
            (PieceTypes.Rook, true) => "♖",
            (PieceTypes.Rook, false) => "♜",
            (PieceTypes.Bishop, true) => "♗",
            (PieceTypes.Bishop, false) => "♝",
            (PieceTypes.Knight, true) => "♘",
            (PieceTypes.Knight, false) => "♞",
            (PieceTypes.Pawn, true) => "♙",
            (PieceTypes.Pawn, false) => "♟",
            _ => string.Empty
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
