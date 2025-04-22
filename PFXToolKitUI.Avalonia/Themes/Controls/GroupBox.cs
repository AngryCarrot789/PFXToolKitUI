// 
// Copyright (c) 2024-2025 REghZy
// 
// This file is part of FramePFX.
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
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace PFXToolKitUI.Avalonia.Themes.Controls;

/// <summary>
/// A headered content control which has a piece of text at the top left, and content below
/// </summary>
public class GroupBox : HeaderedContentControl {
    public static readonly StyledProperty<IBrush> HeaderBrushProperty = AvaloniaProperty.Register<GroupBox, IBrush>("HeaderBrush", Brushes.Transparent);
    public static readonly StyledProperty<double> HeaderContentGapProperty = AvaloniaProperty.Register<GroupBox, double>("HeaderContentGap", 1.0);
    public static readonly StyledProperty<HorizontalAlignment> HorizontalHeaderAlignmentProperty = AvaloniaProperty.Register<GroupBox, HorizontalAlignment>(nameof(HorizontalHeaderAlignment), HorizontalAlignment.Left);
    public static readonly StyledProperty<VerticalAlignment> VerticalHeaderAlignmentProperty = AvaloniaProperty.Register<GroupBox, VerticalAlignment>(nameof(VerticalHeaderAlignment), VerticalAlignment.Center);

    public IBrush HeaderBrush {
        get => this.GetValue(HeaderBrushProperty);
        set => this.SetValue(HeaderBrushProperty, value);
    }

    public double HeaderContentGap {
        get => this.GetValue(HeaderContentGapProperty);
        set => this.SetValue(HeaderContentGapProperty, value);
    }
    
    public HorizontalAlignment HorizontalHeaderAlignment {
        get => this.GetValue(HorizontalHeaderAlignmentProperty);
        set => this.SetValue(HorizontalHeaderAlignmentProperty, value);
    }
    
    public VerticalAlignment VerticalHeaderAlignment {
        get => this.GetValue(VerticalHeaderAlignmentProperty);
        set => this.SetValue(VerticalHeaderAlignmentProperty, value);
    }

    public GroupBox() {
    }
}