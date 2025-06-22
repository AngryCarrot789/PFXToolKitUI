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

using Avalonia.Controls;
using PFXToolKitUI.Avalonia.Services.Windowing;
using PFXToolKitUI.Configurations;
using PFXToolKitUI.Utils.Commands;

namespace PFXToolKitUI.Avalonia.Configurations;

public partial class ConfigurationDialogView : UserControl {
    internal readonly ConfigurationManager configManager;
    internal readonly AsyncRelayCommand ApplyCommand;
    internal readonly AsyncRelayCommand ApplyThenCloseCommand;
    internal readonly AsyncRelayCommand CancelCommand;

    public ConfigurationDialogView(ConfigurationManager manager) {
        this.InitializeComponent();
        this.configManager = manager;

        this.PART_ApplyButton.IsEnabled = true;
        this.PART_ConfirmButton.IsEnabled = true;
        this.PART_CancelButton.IsEnabled = true;
        this.PART_EditorPanel.IsEnabled = true;

        this.ApplyCommand = new AsyncRelayCommand(async () => {
            this.UpdateCommands();
            
            this.PART_EditorPanel.IsEnabled = false;
            await this.configManager.ApplyChangesInHierarchyAsync(null);
            this.PART_EditorPanel.IsEnabled = true;
            
            ApplicationPFX.Instance.Dispatcher.Post(this.UpdateCommands);
        }, () => {
            if (this.ApplyThenCloseCommand!.IsRunning || this.CancelCommand!.IsRunning)
                return false;
            ConfigurationContext? ctx = this.PART_EditorPanel.ActiveContext;
            return ctx != null && ctx.ModifiedPages.Any();
        });

        this.ApplyThenCloseCommand = new AsyncRelayCommand(async () => {
            this.UpdateCommands();
            
            this.PART_EditorPanel.IsEnabled = false;
            await this.configManager.ApplyChangesInHierarchyAsync(null);
            if (TopLevel.GetTopLevel(this) is DesktopWindow window) {
                window.Close(true);
            }
        }, () => !this.ApplyCommand.IsRunning && !this.CancelCommand!.IsRunning);

        this.CancelCommand = new AsyncRelayCommand(async () => {
            this.UpdateCommands();
            this.PART_EditorPanel.IsEnabled = false;
            await this.configManager.RevertLiveChangesInHierarchyAsync(null);
            if (TopLevel.GetTopLevel(this) is DesktopWindow window) {
                window.Close(false);
            }
        }, () => !this.ApplyCommand.IsRunning && !this.ApplyThenCloseCommand.IsRunning);

        this.PART_ApplyButton.Command = this.ApplyCommand;
        this.PART_ConfirmButton.Command = this.ApplyThenCloseCommand;
        this.PART_CancelButton.Command = this.CancelCommand;

        this.PART_EditorPanel.ActiveContextChanged += this.OnEditorContextChanged;
        this.PART_EditorPanel.ConfigurationManager = manager;

        this.UpdateCommands();
    }

    private void OnEditorContextChanged(ConfigurationPanelControl sender, ConfigurationContext? oldContext, ConfigurationContext? newContext) {
        if (oldContext != null)
            oldContext.ModifiedPagesUpdated -= this.OnModifiedPagesChanged;

        if (newContext != null)
            newContext.ModifiedPagesUpdated += this.OnModifiedPagesChanged;

        this.UpdateCommands();
    }

    private void OnModifiedPagesChanged(ConfigurationContext context) {
        this.UpdateCommands();
    }

    private void UpdateCommands() {
        this.ApplyCommand.RaiseCanExecuteChanged();
        this.ApplyThenCloseCommand.RaiseCanExecuteChanged();
        this.CancelCommand.RaiseCanExecuteChanged();
    }
}