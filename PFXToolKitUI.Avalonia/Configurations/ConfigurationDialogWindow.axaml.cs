// 
// Copyright (c) 2023-2025 REghZy
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

using Avalonia.Controls;
using Avalonia.Input;
using PFXToolKitUI.Avalonia.Services.Windowing;
using PFXToolKitUI.Configurations;

namespace PFXToolKitUI.Avalonia.Configurations;

public partial class ConfigurationDialogWindow : DesktopWindow {
    private readonly ConfigurationManager configurationManager;
    private readonly ConfigurationDialogView cdv;

    public ConfigurationDialogWindow(ConfigurationManager configurationManager) {
        this.InitializeComponent();
        this.Content = this.cdv = new ConfigurationDialogView(this.configurationManager = configurationManager);
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);
        if (!e.Handled && e.Key == Key.Escape) {
            e.Handled = true;
            this.cdv.CancelCommand.Execute(null);
        }
    }

    protected override async Task<bool> OnClosingAsync(WindowCloseReason reason) {
        if (await base.OnClosingAsync(reason)) {
            return true;
        }
        
        await this.configurationManager.RevertLiveChangesInHierarchyAsync(null);
        this.cdv.PART_EditorPanel.ConfigurationManager = null;
        return false;
    }
}