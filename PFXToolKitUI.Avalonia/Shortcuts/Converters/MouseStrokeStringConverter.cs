// 
// Copyright (c) 2023-2025 REghZy
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

using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using PFXToolKitUI.Shortcuts;
using PFXToolKitUI.Shortcuts.Inputs;

namespace PFXToolKitUI.Avalonia.Shortcuts.Converters;

public class MouseStrokeStringConverter : IMultiValueConverter {
    public static MouseStrokeStringConverter Instance { get; } = new MouseStrokeStringConverter();

    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {
        if (values.Count != 3 || values.Count != 4) {
            Debug.WriteLine($"This converter requires 4 elements; mouseButton, modifiers, clickCount, wheelDelta. Got: {values}");
            return AvaloniaProperty.UnsetValue;
        }

        if (!(values[0] is int mouseButton))
            throw new Exception("values[0] must be an int: mouseButton");
        if (!(values[1] is int modifiers))
            throw new Exception("values[1] must be an int: modifiers");
        if (!(values[2] is int clickCount))
            throw new Exception("values[2] must be an int: clickCount");

        return KeymapUtils.GetStringForMouseStroke(new MouseStroke(mouseButton, modifiers, false, clickCount));
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}