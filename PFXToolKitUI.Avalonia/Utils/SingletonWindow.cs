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

using System.Diagnostics;
using PFXToolKitUI.Avalonia.Services.Windowing;

namespace PFXToolKitUI.Avalonia.Utils;

/// <summary>
/// A helper class for managing a single view of a window
/// </summary>
public sealed class SingletonWindow {
    private readonly Func<DesktopWindow> factory;
    private readonly WindowingSystem system;
    private DesktopWindow? window;

    public SingletonWindow(Func<DesktopWindow> factory) {
        if (!WindowingSystem.TryGetInstance(out WindowingSystem? windowingSystem)) {
            throw new Exception("No windowing system available");
        }

        this.factory = factory;
        this.system = windowingSystem;
    }

    public void ShowOrActivate() {
        if (this.window == null || !this.window.IsOpen) {
            this.window = this.factory();
            this.window.Closed += this.OnWindowClosed;
            
            this.system.Register(this.window).Show();
        }
        else {
            this.window.Activate();
        }
    }

    private void OnWindowClosed(object? sender, EventArgs e) {
        ((DesktopWindow) sender!).Closed -= this.OnWindowClosed;
        this.window = null;
    }

    public void Close() {
        this.window?.Close();
        Debug.Assert(this.window == null);
    }
}