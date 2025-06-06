﻿// 
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
using Avalonia.Interactivity;
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
            this.PART_ApplyButton.IsEnabled = false;
            this.PART_ConfirmButton.IsEnabled = false;
            this.PART_CancelButton.IsEnabled = false;
            this.PART_EditorPanel.IsEnabled = false;
            await this.configManager.ApplyChangesInHierarchyAsync(null);
            this.PART_ApplyButton.IsEnabled = true;
            this.PART_ConfirmButton.IsEnabled = true;
            this.PART_CancelButton.IsEnabled = true;
            this.PART_EditorPanel.IsEnabled = true;
            this.UpdateConfirm();
        });

        this.ApplyThenCloseCommand = new AsyncRelayCommand(async () => {
            this.PART_ApplyButton.IsEnabled = false;
            this.PART_ConfirmButton.IsEnabled = false;
            this.PART_CancelButton.IsEnabled = false;
            this.PART_EditorPanel.IsEnabled = false;

            await this.configManager.ApplyChangesInHierarchyAsync(null);
            if (TopLevel.GetTopLevel(this) is DesktopWindow window) {
                window.Close(true);
            }
        });

        this.CancelCommand = new AsyncRelayCommand(async () => {
            this.PART_ApplyButton.IsEnabled = false;
            this.PART_ConfirmButton.IsEnabled = false;
            this.PART_CancelButton.IsEnabled = false;
            this.PART_EditorPanel.IsEnabled = false;

            await this.configManager.RevertLiveChangesInHierarchyAsync(null);
            if (TopLevel.GetTopLevel(this) is DesktopWindow window) {
                window.Close(false);
            }
        });

        this.PART_ApplyButton.Command = this.ApplyCommand;
        this.PART_ConfirmButton.Command = this.ApplyThenCloseCommand;
        this.PART_CancelButton.Command = this.CancelCommand;

        this.PART_EditorPanel.ActiveContextChanged += this.OnEditorContextChanged;
        this.PART_EditorPanel.ConfigurationManager = manager;
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => {
            this.PART_EditorPanel.SelectFirst();
        }, DispatchPriority.Loaded);
    }

    private void OnEditorContextChanged(ConfigurationPanelControl sender, ConfigurationContext? oldContext, ConfigurationContext? newContext) {
        if (oldContext != null)
            oldContext.ModifiedPagesUpdated -= this.OnModifiedPagesChanged;

        if (newContext != null)
            newContext.ModifiedPagesUpdated += this.OnModifiedPagesChanged;

        this.UpdateConfirm();
    }

    private void OnModifiedPagesChanged(ConfigurationContext context) {
        this.UpdateConfirm();
    }

    private void UpdateConfirm() {
        ConfigurationContext? ctx = this.PART_EditorPanel.ActiveContext;
        this.PART_ConfirmButton.IsEnabled = ctx != null && ctx.ModifiedPages.Any();
    }
}