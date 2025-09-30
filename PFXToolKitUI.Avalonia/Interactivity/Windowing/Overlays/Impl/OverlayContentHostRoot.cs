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

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Utils;

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing.Overlays.Impl;

public class OverlayContentHostRoot : ContentControl {
    public OverlayWindowManagerImpl Manager { get; private set; } = null!;

    private Panel? PART_Panel;
    private readonly List<OverlayControl> preTemplateItems = new List<OverlayControl>();

    public OverlayContentHostRoot() {
    }

    static OverlayContentHostRoot() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_Panel = e.NameScope.GetTemplateChild<Panel>(nameof(this.PART_Panel));
        foreach (OverlayControl popup in this.preTemplateItems) {
            this.PART_Panel.Children.Add(popup);
        }
    }

    internal void SetupForPopupDialogManager(OverlayWindowManagerImpl overlayManager) {
        this.Manager = overlayManager;
    }

    public void AddPopupToVisualTree(OverlayWindowImpl overlayWindow) {
        if (this.PART_Panel == null) {
            this.preTemplateItems.Add(overlayWindow.myControl);
        }
        else {
            this.PART_Panel.Children.Add(overlayWindow.myControl);
        }

        overlayWindow.myControl.Loaded += this.MyControlOnLoaded;
    }

    public void RemovePopupFromVisualTree(OverlayWindowImpl overlayWindow) {
        if (this.PART_Panel == null) {
            this.preTemplateItems.Remove(overlayWindow.myControl);
        }
        else {
            this.PART_Panel.Children.Remove(overlayWindow.myControl);
        }
        
        overlayWindow.myControl.Loaded -= this.MyControlOnLoaded;
    }

    private void MyControlOnLoaded(object? sender, RoutedEventArgs e) {
        // TODO: measure control to figure out ideal size then force resize
        // PopupOverlayControlImpl control = (PopupOverlayControlImpl) sender!;
        // SizeToContent size = control.overlayWindow.AutoSizeToContent;
        // if (size != SizeToContent.Manual) {
        //     
        // }
    }
}