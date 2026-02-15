using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Rudzoft.ChessLib.Types;

namespace AutoChess.ViewModels;

public sealed class SquareViewModel : INotifyPropertyChanged
{

    private static readonly SolidColorBrush WhitePieceBrush = CreateFrozenBrush(Color.FromRgb(0xee, 0xee, 0xee));
    private static readonly SolidColorBrush BlackPieceBrush = CreateFrozenBrush(Color.FromRgb(0x80, 0x80, 0x80));

    private Piece _piece;
    private bool _isSelected;

    public SquareViewModel(Square square, int rankIndex, int fileIndex)
    {
        Square = square;
        RankIndex = rankIndex;
        FileIndex = fileIndex;
        _piece = Piece.EmptyPiece;
    }

    public Square Square { get; }

    public int RankIndex { get; }

    public int FileIndex { get; }

    public bool IsLightSquare => (RankIndex + FileIndex) % 2 != 0;

    public Piece Piece
    {
        get => _piece;
        set
        {
            if (_piece == value)
            {
                return;
            }

            _piece = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Foreground));
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
            {
                return;
            }

            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public Brush Foreground
    {
        get
        {
            if (Piece == Piece.EmptyPiece)
            {
                return Brushes.Transparent;
            }

            return Piece.ColorOf() == Player.White ? WhitePieceBrush : BlackPieceBrush;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private static SolidColorBrush CreateFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        if (brush.CanFreeze)
        {
            brush.Freeze();
        }

        return brush;
    }
}
