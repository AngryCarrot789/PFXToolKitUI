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

namespace PFXToolKitUI.Avalonia.Converters;

public class FloatToDoubleConverter : IValueConverter {
    public static FloatToDoubleConverter Instance { get; } = new FloatToDoubleConverter();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        switch (value) {
            case float f:  return (double) f;
            case double _: return value;
            default:       return value;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        switch (value) {
            case double d: return (float) d;
            case float _:  return value;
            default:       return value;
        }
    }
}