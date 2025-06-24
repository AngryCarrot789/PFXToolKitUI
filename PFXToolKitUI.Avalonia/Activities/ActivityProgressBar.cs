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

using Avalonia;
using Avalonia.Controls;

namespace PFXToolKitUI.Avalonia.Activities;

public class ActivityProgressBar : ProgressBar {
    public static readonly StyledProperty<double?> InitialWidthProperty = AvaloniaProperty.Register<ActivityProgressBar, double?>(nameof(InitialWidth));
    
    protected override Type StyleKeyOverride => typeof(ProgressBar);

    /// <summary>
    /// Gets or sets the value we try to fill initially. Null disabled the feature
    /// </summary>
    public double? InitialWidth {
        get => this.GetValue(InitialWidthProperty);
        set => this.SetValue(InitialWidthProperty, value);
    }
    
    protected override Size MeasureOverride(Size availableSize) {
        Size size = base.MeasureOverride(availableSize);
        if (this.InitialWidth is double width) {
            double newWidth = Math.Min(availableSize.Width, width);
            // Console.WriteLine($"[Measure] availableSize = {availableSize}, size = {size}, new width = {newWidth}");
            size = size.WithWidth(newWidth);
        }
        else {
            // Console.WriteLine($"[Measure] availableSize = {availableSize}, size = {size}");
        }

        return size;
    }

    protected override Size ArrangeOverride(Size finalSize) {
        Size size = base.ArrangeOverride(finalSize);
        // if (this.InitialWidth is double width) {
        //     // double newWidth = Math.Min(finalSize.Width, width);
        //     // Console.WriteLine($"[Arrange] finalSize = {finalSize}, size = {size}, new width = {newWidth}");
        //     // size = size.WithWidth(newWidth);
        // }
        // else {
        //     // Console.WriteLine($"[Arrange] finalSize = {finalSize}, size = {size}");
        // }

        return size;
    }
}