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
using Avalonia.Controls;

namespace PFXToolKitUI.Avalonia.AvControls.ListBoxes;

public class BaseModelBasedListBox : ListBox {
    public static readonly StyledProperty<bool> CanDragItemPositionProperty = AvaloniaProperty.Register<BaseModelBasedListBox, bool>(nameof(CanDragItemPosition));

    public bool CanDragItemPosition {
        get => this.GetValue(CanDragItemPositionProperty);
        set => this.SetValue(CanDragItemPositionProperty, value);
    }

    public BaseModelBasedListBox() {
    }

    internal virtual void MoveItemIndex(int oldIndex, int newIndex) {
        
    }
}