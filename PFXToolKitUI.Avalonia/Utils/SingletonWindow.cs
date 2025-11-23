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

using PFXToolKitUI.Avalonia.Interactivity.Windowing;
using PFXToolKitUI.Avalonia.Interactivity.Windowing.Desktop;

namespace PFXToolKitUI.Avalonia.Utils;

/// <summary>
/// A helper class for managing a single view of a window
/// </summary>
public sealed class SingletonWindow {
    private readonly IWindowManager? manager;
    private readonly Func<IWindowManager, IDesktopWindow> factory;
    
    /// <summary>
    /// Gets the current window
    /// </summary>
    public IDesktopWindow? Current { get; private set; }

    /// <summary>
    /// A helper class for managing a single view of a window
    /// </summary>
    public SingletonWindow(Func<IWindowManager, IDesktopWindow> factory) {
        this.factory = factory;
        if (!IWindowManager.TryGetInstance(out this.manager))
            throw new InvalidOperationException("No window manager available");
    }

    public void ShowOrActivate() {
        if (this.Current != null) {
            if (this.Current.OpenState.IsOpenOrTryingToClose()) {
                this.Current.Activate();
            }
        }
        else {
            this.Current = this.factory(this.manager!);
            this.Current.Closed += this.OnWindowClosed;
            this.Current.ShowAsync();
        }
    }

    private void OnWindowClosed(object? s, WindowCloseEventArgs args) {
        ((IDesktopWindow) s!).Closed -= this.OnWindowClosed;
        this.Current = null;
    }
}