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

using PFXToolKitUI.Avalonia.Interactivity.Windowing;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Themes;
using SkiaSharp;

namespace PFXToolKitUI.Avalonia.Services;

public class LogViewServiceImpl : ILogViewService {
    private IWindow? currentWindow;

    public Task ShowLogsWindow() {
        if (IWindowManager.TryGetInstance(out IWindowManager? manager)) {
            if (this.currentWindow == null) {
                this.currentWindow = manager.CreateWindow(new WindowBuilder() {
                    Title = "Application Logs",
                    Content = new LogsView(),
                    TitleBarBrush = BrushManager.Instance.GetDynamicThemeBrush("ABrush.Tone4.Background.Static"),
                    BorderBrush = BrushManager.Instance.CreateConstant(SKColors.DodgerBlue),
                    Width = 900, Height = 700
                });

                this.currentWindow.WindowOpened += static (sender, args) => ((LogsView) sender.Content!).OnWindowOpened();
                this.currentWindow.WindowClosed += (sender, args) => {
                    ((LogsView) sender.Content!).OnWindowClosed();
                    this.currentWindow = null;
                };
                
                return this.currentWindow.ShowAsync();
            }

            this.currentWindow.Activate();
            return Task.CompletedTask;
        }
        
        return Task.CompletedTask;
    }
}