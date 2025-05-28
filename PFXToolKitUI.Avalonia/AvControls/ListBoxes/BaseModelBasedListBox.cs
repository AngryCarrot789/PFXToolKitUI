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
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;

namespace PFXToolKitUI.Avalonia.AvControls.ListBoxes;

/// <summary>
/// The base non-generic version of <see cref="ModelBasedListBox{TModel}"/>
/// </summary>
public class BaseModelBasedListBox : ListBox {
    public static readonly StyledProperty<bool> CanDragItemPositionProperty = AvaloniaProperty.Register<BaseModelBasedListBox, bool>(nameof(CanDragItemPosition));
    
    /// <summary>
    /// Gets or sets whether dragging items is allowed. Even when true, dragging items may not be possible
    /// if it's not implemented (see <see cref="MoveItemIndex"/> and <see cref="CanDragItemPositionCore"/>)
    /// </summary>
    public bool CanDragItemPosition {
        get => this.GetValue(CanDragItemPositionProperty);
        set => this.SetValue(CanDragItemPositionProperty, value);
    }

    /// <summary>
    /// Indicates whether calling <see cref="MoveItemIndex"/> will actually do anything. By default, it
    /// invokes <see cref="MoveItemHandler"/> therefore this property checks if the handler is non-null.
    /// <para>
    /// If a custom implementation of <see cref="MoveItemIndex"/> is used, this property should be overridden
    /// </para>
    /// </summary>
    protected virtual bool CanDragItemPositionCore => false;

    /// <summary>
    /// Returns true when dragging items is actually possible and calls to <see cref="MoveItemIndex"/>
    /// will most likely result in the item being moved
    /// </summary>
    public bool CanEffectivelyDragItemPosition => this.CanDragItemPosition && this.CanDragItemPositionCore;
    
    public BaseModelBasedListBox() {
    }

    static BaseModelBasedListBox() {
        ItemsPanelProperty.OverrideDefaultValue<BaseModelBasedListBox>(new FuncTemplate<Panel?>(() => new StackPanel()));
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        if (this.ItemsPanelRoot is VirtualizingPanel) {
            throw new InvalidOperationException("Cannot use a virtualizing panel on a model based list box");
        }
    }

    /// <summary>
    /// Actually moves the items in this list box. By default, this does nothing. When you override this,
    /// make sure to override <see cref="CanDragItemPositionCore"/> to return true when possible
    /// </summary>
    /// <param name="oldIndex">Source index</param>
    /// <param name="newIndex">Destination index</param>
    protected internal virtual void MoveItemIndex(int oldIndex, int newIndex) {
        
    }
}