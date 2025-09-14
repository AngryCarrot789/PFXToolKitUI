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
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace PFXToolKitUI.Avalonia.Themes.Converters;

public sealed class IconImageConverter : IValueConverter {
    public static IconImageConverter Instance { get; } = new IconImageConverter();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is WindowIcon icon) {
            return WindowIconToBitmap(icon);
        }

        return value == AvaloniaProperty.UnsetValue ? value : null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

    public static Bitmap WindowIconToBitmap(WindowIcon icon) {
        using MemoryStream stream = new MemoryStream(1024);
        icon.Save(stream);
        stream.Seek(0, SeekOrigin.Begin);
        return new Bitmap(stream);
    }
}