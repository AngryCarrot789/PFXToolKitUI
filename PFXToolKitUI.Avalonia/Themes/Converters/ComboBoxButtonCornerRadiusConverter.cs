using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace PFXToolKitUI.Avalonia.Themes.Converters;

public class ComboBoxButtonCornerRadiusConverter : IValueConverter {
    public static ComboBoxButtonCornerRadiusConverter Instance { get; } = new ComboBoxButtonCornerRadiusConverter();

    private ComboBoxButtonCornerRadiusConverter() {
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value == AvaloniaProperty.UnsetValue)
            return value;
        if (!(value is CornerRadius r))
            throw new ArgumentException($"Invalid value, expected {nameof(CornerRadius)}, got {value?.GetType().Name}");

        return new CornerRadius(0, r.TopRight, r.BottomRight, 0);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}