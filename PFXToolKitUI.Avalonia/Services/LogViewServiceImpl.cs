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

using PFXToolKitUI.Avalonia.Services.Windowing;
using PFXToolKitUI.Logging;

namespace PFXToolKitUI.Avalonia.Services;

public class LogViewServiceImpl : ILogViewService {
    private WeakReference<LogsWindow>? currentWindow;

    public Task ShowLogsWindow() {
        if (WindowingSystem.TryGetInstance(out WindowingSystem? system)) {
            if (this.currentWindow == null || !this.currentWindow.TryGetTarget(out LogsWindow? existing) || existing.IsClosed) {
                LogsWindow window = new LogsWindow();
                if (this.currentWindow == null)
                    this.currentWindow = new WeakReference<LogsWindow>(window);
                else
                    this.currentWindow.SetTarget(window);

                system.Register(window).Show();
            }
            else {
                existing.Activate();
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
}