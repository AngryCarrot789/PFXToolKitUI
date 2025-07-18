﻿// 
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

using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using PFXToolKitUI.Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Shortcuts.Avalonia;
using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.Avalonia.AdvancedMenuService;

/// <summary>
/// A menu that captures the context data of the focused element just before the menu is opened
/// </summary>
public class ContextCapturingMenu : Menu {
    private static readonly AttachedProperty<IContextData?> CapturedContextProperty = AvaloniaProperty.RegisterAttached<ContextCapturingMenu, AvaloniaObject, IContextData?>("CapturedContext", inherits: true);

    private InputElement? lastFocus;
    protected override Type StyleKeyOverride => typeof(Menu);

    public ContextCapturingMenu() {
    }

    public override void Close() {
        bool wasOpen = this.IsOpen;
        base.Close();
        if (wasOpen && this.lastFocus != null) {
            this.ClearValue(CapturedContextProperty);
            DataManager.ClearContextData(this);
            Debug.WriteLine("Cleared captured data context");
            if (this.lastFocus != this) {
                this.lastFocus.Focus();
            }

            this.lastFocus = null;
        }
    }

    protected override void OnGotFocus(GotFocusEventArgs e) {
        this.lastFocus = null;
        if (TopLevel.GetTopLevel(this) is TopLevel topLevel) {
            this.lastFocus = UIInputManager.GetLastFocusedElement(topLevel);
        }

        base.OnGotFocus(e);
        if (this.lastFocus != null) {
            this.CaptureContextFromObject(this.lastFocus);
        }
    }

    private void CaptureContextFromObject(InputElement inputElement) {
        IContextData ctx = DataManager.GetFullContextData(inputElement);
        Debug.WriteLine($"Captured context{(ctx is IRandomAccessContextData data ? $" ({data.Count} entries) " : " ")}before menu focus switch from {inputElement.GetType().Name}");
        this.SetValue(CapturedContextProperty, ctx);
        DataManager.SwapDelegateContextData(this, ctx);
    }

    public static IContextData? GetCapturedContext(AvaloniaObject obj) => obj.GetValue(CapturedContextProperty);
}