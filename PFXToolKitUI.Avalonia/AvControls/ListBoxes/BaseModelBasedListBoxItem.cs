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
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.AvControls.ListBoxes;

public class BaseModelBasedListBoxItem : ListBoxItem {
    private PixelPoint lastMovePosAbs;
    private Point leftClickPos;
    private bool isMovingBetweenTracks;
    internal IPointer? initiatedDragPointer;

    private double height1, height2;

    public BaseModelBasedListBox? ListBox { get; internal set; }

    public BaseModelBasedListBoxItem() {
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        if (!e.Handled) {
            PointerPoint point = e.GetCurrentPoint(this);
            if (point.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed) {
                this.leftClickPos = e.GetPosition(this);
                this.lastMovePosAbs = this.PointToScreen(this.leftClickPos);
                this.initiatedDragPointer = e.Pointer;
            }
        }
        
        base.OnPointerPressed(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e) {
        base.OnPointerReleased(e);
        this.initiatedDragPointer = null;
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        Dispatcher.UIThread.InvokeAsync(() => {
            if (this.isMovingBetweenTracks) {
                this.isMovingBetweenTracks = false;
                this.Focus(NavigationMethod.Pointer);
                this.initiatedDragPointer?.Capture(this);
            }
        }, DispatcherPriority.Send);
    }

    protected override void OnPointerMoved(PointerEventArgs e) {
        base.OnPointerMoved(e);
        if (e.Handled) {
            return;
        }
        
        PointerPoint point = e.GetCurrentPoint(this);
        Point mPos = e.GetPosition(this);

        // This is used to prevent "drag jumping" which occurs when a screen pixel is
        // somewhere in between a frame in a sweet spot that results in the control
        // jumping back and forth. To test, do CTRL+MouseWheelUp once to zoom in a bit,
        // and then drag a clip 1 frame at a time and you might see it with the code below removed.
        // This code is pretty much the exact same as what Thumb uses
        PixelPoint mPosAbs = this.PointToScreen(mPos);
        bool hasMovedX = !DoubleUtils.AreClose(mPosAbs.X, this.lastMovePosAbs.X);
        bool hasMovedY = !DoubleUtils.AreClose(mPosAbs.Y, this.lastMovePosAbs.Y);
        if (!hasMovedX && !hasMovedY) {
            return;
        }

        this.lastMovePosAbs = mPosAbs;
        if (!point.Properties.IsLeftButtonPressed || this.initiatedDragPointer == null) {
            return;
        }

        if (this.ListBox?.CanDragItemPosition != true) {
            return;
        }

        Vector mPosDiffRel = mPos - this.leftClickPos;
        if (hasMovedY && !this.isMovingBetweenTracks && Math.Abs(mPosDiffRel.Y) >= 1.0d) {
            int srcIdx = this.ListBox.Items.IndexOf(this);
            Point pointerPos = e.GetPosition(this.ListBox);
            IEnumerable<Visual> visuals = this.ListBox.GetVisualsAt(pointerPos);
            foreach (Visual visual in visuals) {
                BaseModelBasedListBoxItem? listItem = VisualTreeUtils.GetParent<BaseModelBasedListBoxItem>(visual);
                if (listItem != null && listItem != this) {
                    int dstIdx = this.ListBox.Items.IndexOf(listItem);
                    if (srcIdx == dstIdx) {
                        break;
                    }

                    bool isMovingDown = dstIdx > srcIdx;
                    Point mPosItemRel2List = e.GetPosition(listItem);
                    double threshold = listItem.Bounds.Height / 2;
                    if ((isMovingDown && mPosItemRel2List.Y > threshold) || (!isMovingDown && mPosItemRel2List.Y < threshold)) {
                        this.isMovingBetweenTracks = true;
                        e.Pointer.Capture(null);
                        this.ListBox.MoveItemIndex(srcIdx, dstIdx);
                        e.Handled = true;
                    }
                }
            }

            // double spacing = 0, totalHeight = 0.0;
            // Panel? panel = this.ListBox.ItemsPanelRoot;
            // if (panel is StackPanel) {
            //     spacing = ((StackPanel) panel).Spacing;
            // }
            // 
            // List<BaseModelBasedListBoxItem> items = new List<BaseModelBasedListBoxItem>(this.ListBox.Items.Cast<BaseModelBasedListBoxItem>());
            // Point mPosList = e.GetPosition(this.ListBox);
            // int srcIdx = this.ListBox.Items.IndexOf(this);
            // for (int i = 0, endIndex = items.Count - 1; i <= endIndex; i++) {
            //     BaseModelBasedListBoxItem item = items[i];
            //     if (item != this && DoubleUtils.GreaterThanOrClose(mPosList.Y, totalHeight) && DoubleUtils.LessThanOrClose(mPosList.Y, totalHeight + item.Bounds.Height)) {
            //         int dstIdx = this.ListBox.Items.IndexOf(item);
            //         if (srcIdx != dstIdx) {
            //             this.isMovingBetweenTracks = true;
            //             e.Pointer.Capture(null);
            //             this.ListBox.MoveItemIndex(srcIdx, dstIdx);
            //             e.Handled = true;
            //             break;
            //         }
            //     }
            //
            //     // 1.0 includes the gap between tracks
            //     totalHeight += item.Bounds.Height + spacing;
            // }
        }
    }
}