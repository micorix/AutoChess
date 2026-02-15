using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AutoChess.Converters;

public sealed class SquareBackgroundConverter : IMultiValueConverter
{
    public Brush LightBrush { get; set; } = CreateFrozenBrush(Color.FromRgb(0x28, 0x28, 0x28));
    public Brush DarkBrush { get; set; } = CreateFrozenBrush(Color.FromRgb(0x1e, 0x1e, 0x1e));
    public Brush SelectedBrush { get; set; } = CreateFrozenBrush(Color.FromRgb(0xfa, 0xb2, 0x83));

    private static bool IsTrue(object[] values, int index) => values.Length > index && values[index] is bool value && value;

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var isSelected = IsTrue(values, 0);
        var isLight = IsTrue(values, 1);

        if (isSelected)
        {
            return SelectedBrush;
        }

        return isLight ? LightBrush : DarkBrush;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();

    private static SolidColorBrush CreateFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
