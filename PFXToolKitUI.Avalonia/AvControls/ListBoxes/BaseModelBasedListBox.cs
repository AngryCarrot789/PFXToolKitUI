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

using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;

namespace PFXToolKitUI.Avalonia.AvControls.ListBoxes;

/// <summary>
/// The base non-generic version of <see cref="ModelBasedListBox{TModel}"/>
/// </summary>
public abstract class BaseModelBasedListBox : ListBox {
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
    /// Indicates whether calling <see cref="MoveItemIndex"/> will actually do anything.
    /// <para>
    /// If <see cref="MoveItemIndex"/> is overridden, this property should be overridden
    /// </para>
    /// </summary>
    protected virtual bool CanDragItemPositionCore => false;

    /// <summary>
    /// Returns true when dragging items is actually possible and calls to <see cref="MoveItemIndex"/>
    /// will most likely result in the item being moved
    /// </summary>
    public bool CanEffectivelyDragItemPosition => this.CanDragItemPosition && this.CanDragItemPositionCore;

    /// <summary>
    /// Gets the item currently being dragged
    /// </summary>
    protected BaseModelBasedListBoxItem? CurrentDragItem { get; private set; }

    private bool isMovingItem;
    
    protected BaseModelBasedListBox() {
    }

    static BaseModelBasedListBox() {
        ItemsPanelProperty.OverrideDefaultValue<BaseModelBasedListBox>(new FuncTemplate<Panel?>(() => new StackPanel()));
    }

    protected void InternalOnRemovingItem(BaseModelBasedListBoxItem item) {
        if (this.CurrentDragItem == item && !this.isMovingItem) {
            Debug.Assert(item.ListBox == this);
            this.CurrentDragItem.ForceStopDrag();
            Debug.Assert(this.CurrentDragItem == null);
        }
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        if (this.ItemsPanelRoot is VirtualizingPanel) {
            throw new InvalidOperationException("Cannot use a virtualizing panel on a model based list box");
        }
    }

    internal void BeginDrag(BaseModelBasedListBoxItem item) {
        if (this.CurrentDragItem != null) {
            Debug.Fail("Already dragging an item");
            this.OnDragItemEnd(this.CurrentDragItem);
        }

        this.OnDragItemBegin(this.CurrentDragItem = item);
    }

    internal void EndDrag(BaseModelBasedListBoxItem item) {
        if (this.CurrentDragItem != item) {
            Debug.Fail("Different drag items");
            this.OnDragItemEnd(this.CurrentDragItem);
        }

        this.OnDragItemEnd(item);
        this.CurrentDragItem = null;
    }

    /// <summary>
    /// Actually moves the items in this list box. By default, this does nothing. When you override this,
    /// make sure to override <see cref="CanDragItemPositionCore"/> to return true when possible
    /// </summary>
    /// <param name="oldIndex">Source index</param>
    /// <param name="newIndex">Destination index</param>
    protected internal void MoveItemIndex(int oldIndex, int newIndex) {
        this.isMovingItem = true;
        this.MoveItemIndexOverride(oldIndex, newIndex);
        this.isMovingItem = false;
    }
    
    protected internal virtual void MoveItemIndexOverride(int oldIndex, int newIndex) {
    }

    protected bool StopCurrentDrag() {
        if (this.CurrentDragItem == null)
            return false;
        
        Debug.Assert(this.CurrentDragItem.ListBox == this);
        
        bool stopped = this.CurrentDragItem.ForceStopDrag();
        Debug.Assert(this.CurrentDragItem == null);
        return stopped;
    }

    /// <summary>
    /// Invoked when the user clicks the item or dragging grip to start moving the item within the list
    /// </summary>
    /// <param name="item">The item being dragged</param>
    protected virtual void OnDragItemBegin(BaseModelBasedListBoxItem item) {
        Debug.WriteLine("Begin dragging item");
    }

    /// <summary>
    /// Invoked when the user releases their cursor to stop moving the item within the list
    /// </summary>
    /// <param name="item">The item no longer being dragged</param>
    protected virtual void OnDragItemEnd(BaseModelBasedListBoxItem item) {
        Debug.WriteLine("End dragging item");
    }
}