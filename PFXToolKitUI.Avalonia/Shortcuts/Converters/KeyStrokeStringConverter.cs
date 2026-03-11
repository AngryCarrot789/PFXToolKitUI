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

using System.Globalization;
using Avalonia.Data.Converters;
using PFXToolKitUI.Shortcuts;
using PFXToolKitUI.Shortcuts.Inputs;

namespace PFXToolKitUI.Avalonia.Shortcuts.Converters;

public class KeyStrokeStringConverter : IMultiValueConverter {
    public static KeyStrokeStringConverter Instance { get; } = new KeyStrokeStringConverter();

    public bool AppendKeyDown { get; set; } = true;
    public bool AppendKeyUp { get; set; } = true;

    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {
        if (values == null || values.Count != 3) {
            throw new Exception("This converter requires 3 elements; keycode, modifiers, isRelease");
        }

        if (!(values[0] is int keyCode))
            throw new Exception("values[0] must be an int: keycode");
        if (!(values[1] is int modifiers))
            throw new Exception("values[1] must be an int: modifiers");
        if (!(values[2] is bool isRelease))
            throw new Exception("values[2] must be a bool: isRelease");

        return KeymapUtils.GetStringForKeyStroke(new KeyStroke(keyCode, modifiers, isRelease), this.AppendKeyDown, this.AppendKeyUp);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}