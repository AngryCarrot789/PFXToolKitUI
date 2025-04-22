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

using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using PFXToolKitUI.Avalonia.AvControls.Dragger;

namespace PFXToolKitUI.Avalonia.Services;

// Current impl is shared across every platform. Could at some point use Win32DesktopServiceImpl, MacDesktopServiceImpl, etc.
public class DesktopServiceImpl : IDesktopService {
    public Application Application { get; }

    public DesktopServiceImpl(Application application) {
        this.Application = application ?? throw new ArgumentNullException(nameof(application));
    }

    // At some point, we need to stop using Window and instead rely on "window" wrappers.
    // This is so that we can still use "windows" on platforms that do not support windows.
    // E.g. using the avalonia linux screen buffer nuget package or whatever it's called, that is a
    // single view application, so Window won't work AFAIK. Our wrapper would be a service like IWindowService,
    // which returns IWindow or IDialogWindow (or both merged maybe... still to-do), which wraps a Window on desktop
    // and wraps a hovering window control (placed ontop of the main view) on single view apps
    public bool TryGetActiveWindow([NotNullWhen(true)] out Window? window, bool fallbackToMainWindow = true) {
        if (this.Application.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            return (window = desktop.Windows.FirstOrDefault(x => x.IsActive) ?? (fallbackToMainWindow ? desktop.MainWindow : null)) != null;
        }

        window = null;
        return false;
    }

    public void SetCursorPosition(int x, int y) {
        if (OperatingSystem.IsWindows()) {
            Win32CursorUtils.SetCursorPos(x, y);
        }
        // TODO: other platforms
    }
}