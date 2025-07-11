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
using Avalonia;
using Avalonia.Data.Converters;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Converters;

public class BoolConverterAND : IMultiValueConverter {
    public bool EmptyArrayBool { get; set; } = false;

    public bool NonBoolBool {
        get => this.NonBoolValue is bool b && b;
        set => this.NonBoolValue = value.Box();
    }

    public object NonBoolValue { get; set; } = AvaloniaProperty.UnsetValue;

    public object Convert(List<object> values, Type targetType, object parameter, CultureInfo culture) {
        // if (values == null || values.Length != 3) {
        //     throw new Exception("Expected 3 elements, not " + (values != null ? values.Length.ToString() : "null"));
        // }
        // bool a = (bool) values[0]; // showOptionAlwaysUseResult
        // bool b = (bool) values[1]; // isAlwaysUseNextResult
        // bool c = (bool) values[2]; // showOptionAlwaysUseResultForCurrent
        // return (a && b && c).Box(); // box utils as optimisation
        if (values == null || values.Count == 0) {
            return this.EmptyArrayBool.Box();
        }

        foreach (object value in values) {
            if (value is bool boolean) {
                if (!boolean)
                    return BoolBox.False;
            }
            else {
                return this.NonBoolValue;
            }
        }

        return BoolBox.True;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}