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
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.AvControls.ListBoxes;

public abstract class BaseModelBasedListBoxItem : ListBoxItem {
    private Control dragInitiator;
    private PixelPoint lastMovePosAbs;
    private Point leftClickPos;
    private bool isMovingBetweenTracks;
    internal IPointer? initiatedDragPointer;

    public BaseModelBasedListBox? ListBox { get; internal set; }

    public BaseModelBasedListBoxItem() {
        this.dragInitiator = this;
        this.HookDragEvents(this);
    }
    
    /// <summary>
    /// Called before this item is added to the <see cref="ListBox"/> but after the <see cref="ListBox"/> and <see cref="Model"/> references are set
    /// </summary>
    protected abstract void OnAddingToList();

    /// <summary>
    /// Invoked once the styling and template is applied and we're added to the <see cref="ListBox"/>.
    /// This can connect binders and add event handlers
    /// </summary>
    protected abstract void OnAddedToList();

    /// <summary>
    /// Invoked when we're about to be removed from our <see cref="ListBox"/>.
    /// This can disconnect binders and remove event handlers
    /// </summary>
    protected abstract void OnRemovingFromList();

    /// <summary>
    /// Invoked once removed from <see cref="ListBox"/> but before the <see cref="ListBox"/> and <see cref="Model"/> references are cleared
    /// </summary>
    protected abstract void OnRemovedFromList();

    /// <summary>
    /// Sets the control that acts as the source for drag initiation. Default is 'this'
    /// </summary>
    /// <param name="control"></param>
    public void SetDragSourceControl(Control control) {
        ArgumentNullException.ThrowIfNull(control);
        this.dragInitiator.Cursor = Cursor.Default;
        this.UnhookDragEvents(this.dragInitiator);
        this.HookDragEvents(this.dragInitiator = control);
    }

    private void UnhookDragEvents(Control control) {
        control.PointerPressed -= this.OnDragSourcePointerPressed;
        control.PointerReleased -= this.OnDragSourcePointerReleased;
        control.PointerMoved -= this.OnDragSourcePointerMoved;
    }

    private void HookDragEvents(Control control) {
        control.PointerPressed += this.OnDragSourcePointerPressed;
        control.PointerReleased += this.OnDragSourcePointerReleased;
        control.PointerMoved += this.OnDragSourcePointerMoved;
    }

    private void OnDragSourcePointerPressed(object? sender, PointerEventArgs e) {
        if (!e.Handled && e.GetCurrentPoint(this.dragInitiator).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed) {
            this.leftClickPos = e.GetPosition(this);
            this.lastMovePosAbs = this.PointToScreen(this.leftClickPos);
            this.initiatedDragPointer = e.Pointer;
        }
    }

    private void OnDragSourcePointerReleased(object? sender, PointerEventArgs e) {
        this.initiatedDragPointer = null;
    }

    private void OnDragSourcePointerMoved(object? sender, PointerEventArgs e) {
        if (this.dragInitiator != this) {
            this.dragInitiator.Cursor = Cursor.Default;
        }

        if (e.Handled) {
            return;
        }

        bool canDrag = this.ListBox?.CanEffectivelyDragItemPosition == true;
        if (!canDrag) {
            this.initiatedDragPointer = null;
            if (this.dragInitiator != this) {
                this.dragInitiator.Cursor = new Cursor(StandardCursorType.No);
            }
        }
        
        PointerPoint point = e.GetCurrentPoint(this);
        Point mPos = e.GetPosition(this);

        // This is used to prevent "drag jumping" which occurs when a screen pixel is
        // somewhere in between a frame in a sweet spot that results in the control
        // jumping back and forth. This code is pretty much the exact same as what Thumb uses
        PixelPoint mPosAbs = this.PointToScreen(mPos);
        bool hasMovedX = !DoubleUtils.AreClose(mPosAbs.X, this.lastMovePosAbs.X);
        bool hasMovedY = !DoubleUtils.AreClose(mPosAbs.Y, this.lastMovePosAbs.Y);
        if (!hasMovedX && !hasMovedY) {
            return;
        }

        this.lastMovePosAbs = mPosAbs;
        if (!canDrag || !point.Properties.IsLeftButtonPressed || this.initiatedDragPointer == null) {
            this.initiatedDragPointer = null;
            return;
        }

        Vector mPosDiffRel = mPos - this.leftClickPos;
        if (hasMovedY && !this.isMovingBetweenTracks && Math.Abs(mPosDiffRel.Y) >= 1.0d) {
            List<BaseModelBasedListBoxItem> items = this.ListBox!.Items.Cast<BaseModelBasedListBoxItem>().ToList();
            int srcIdx = items.IndexOf(this);
            foreach (BaseModelBasedListBoxItem item in items) {
                if (item != this) {
                    int dstIdx = this.ListBox.Items.IndexOf(item);
                    if (srcIdx == dstIdx) {
                        break;
                    }

                    bool isMovingDown = dstIdx > srcIdx;
                    Point mPosItemRel2List = e.GetPosition(item);
                    double threshold = item.Bounds.Height / 2;
                    if ((isMovingDown && mPosItemRel2List.Y > threshold) || (!isMovingDown && mPosItemRel2List.Y < threshold)) {
                        this.isMovingBetweenTracks = true;
                        e.Pointer.Capture(null);
                        this.ListBox.MoveItemIndex(srcIdx, dstIdx);
                        e.Handled = true;
                        break;
                    }
                }
            }
        }
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        Dispatcher.UIThread.InvokeAsync(() => {
            if (this.isMovingBetweenTracks) {
                this.isMovingBetweenTracks = false;
                this.dragInitiator.Focus(NavigationMethod.Pointer);
                this.initiatedDragPointer?.Capture(this.dragInitiator);
            }
        }, DispatcherPriority.Send);
    }
}