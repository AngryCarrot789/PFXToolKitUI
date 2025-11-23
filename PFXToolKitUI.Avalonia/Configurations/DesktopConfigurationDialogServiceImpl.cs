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

using Avalonia.Input;
using PFXToolKitUI.Avalonia.Interactivity.Windowing;
using PFXToolKitUI.Avalonia.Interactivity.Windowing.Desktop;
using PFXToolKitUI.Configurations;
using PFXToolKitUI.Interactivity.Windowing;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Configurations;

public class DesktopConfigurationDialogServiceImpl : IConfigurationDialogService {
    public async Task ShowConfigurationDialog(ConfigurationManager configurationManager) {
        ITopLevel? topLevel = TopLevelContextUtils.GetTopLevelFromContext();
        if (!(topLevel is IDesktopWindow parentWindow))
            return;
        
        IDesktopWindow window = parentWindow.WindowManager.CreateWindow(new WindowBuilder() {
            Title = "Settings",
            Content = new ConfigurationDialogView(configurationManager),
            FocusPath = "Configuration",
            BorderBrush = BrushManager.Instance.GetDynamicThemeBrush("PanelBorderBrush"),
            TitleBarBrush = BrushManager.Instance.GetDynamicThemeBrush("ABrush.Tone7.Background.Static"),
            MinWidth = 600, MinHeight = 450,
            Width = 950, Height = 700,
            Parent = parentWindow
        });

        window.Control.AddHandler(InputElement.KeyDownEvent, OnWindowKeyDown);
        window.Opened += static (s, e) => ((ConfigurationDialogView) ((IDesktopWindow) s!).Content!).Window = (IDesktopWindow) s!;
        window.Closed += static (s, e) => ((ConfigurationDialogView) ((IDesktopWindow) s!).Content!).Window = null;

        window.ClosingAsync += static (s, args) => ApplicationPFX.Instance.Dispatcher.InvokeAsync(async () => {
            ConfigurationDialogView view = (ConfigurationDialogView) ((IDesktopWindow) s!).Content!;
            await view.configManager.RevertLiveChangesInHierarchyAsync(null);
            view.PART_EditorPanel.ConfigurationManager = null;
        }).Unwrap();

        await window.ShowDialogAsync();
        return;

        void OnWindowKeyDown(object? sender, KeyEventArgs e) {
            if (!e.Handled && e.Key == Key.Escape && window.OpenState == OpenState.Open) {
                e.Handled = true;
                ((ConfigurationDialogView) window.Content!).CancelCommand.Execute(null);
            }
        }
    }
}