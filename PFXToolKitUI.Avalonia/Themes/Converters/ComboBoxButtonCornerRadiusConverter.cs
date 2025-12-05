// 
// Copyright (c) 2024-2025 REghZy
// 
// This file is part of PFXToolKitUI.
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3 of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with PFXToolKitUI. If not, see <https://www.gnu.org/licenses/>.
// 

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