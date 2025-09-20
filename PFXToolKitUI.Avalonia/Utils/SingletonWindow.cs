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
using PFXToolKitUI.Avalonia.Interactivity.Windowing;

namespace PFXToolKitUI.Avalonia.Utils;

/// <summary>
/// A helper class for managing a single view of a window
/// </summary>
public sealed class SingletonWindow {
    private readonly IWindowManager? manager;
    private readonly Func<IWindowManager, IWindow> factory;
    
    /// <summary>
    /// Gets the current window
    /// </summary>
    public IWindow? Current { get; private set; }

    /// <summary>
    /// A helper class for managing a single view of a window
    /// </summary>
    public SingletonWindow(Func<IWindowManager, IWindow> factory) {
        this.factory = factory;
        if (!IWindowManager.TryGetInstance(out this.manager))
            throw new InvalidOperationException("No window manager available");
    }

    public void ShowOrActivate() {
        if (this.Current != null) {
            if (this.Current.OpenState == OpenState.Open) {
                this.Current.Activate();
            }
        }
        else {
            this.Current = this.factory(this.manager!);
            this.Current.WindowClosed += this.OnWindowClosed;
            this.Current.ShowAsync();
        }
    }

    private void OnWindowClosed(IWindow sender, EventArgs e) {
        sender.WindowClosed -= this.OnWindowClosed;
        this.Current = null;
    }
}