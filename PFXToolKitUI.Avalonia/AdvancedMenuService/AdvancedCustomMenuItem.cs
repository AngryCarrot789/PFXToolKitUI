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

using System.Diagnostics;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.AdvancedMenuService;

public class AdvancedCustomMenuItem : AdvancedMenuItem {
    private bool canExecute;

    public new CustomContextEntry? Entry => (CustomContextEntry?) base.Entry;

    public bool IsExecuting { get; private set; }

    protected bool CanExecute {
        get => this.canExecute;
        set {
            this.canExecute = value;

            // Causes IsEnableCore to be fetched, which returns false if we are executing something or
            // we have no valid command, causing this menu item to be "disabled"
            this.UpdateIsEffectivelyEnabled();
        }
    }

    protected override bool IsEnabledCore => base.IsEnabledCore && this.CanExecute;

    public AdvancedCustomMenuItem() {
    }

    public override void UpdateCanExecute() {
        if (!this.IsLoaded)
            return;

        if (this.IsExecuting) {
            this.CanExecute = false;
        }
        else if (this.Entry is CustomContextEntry entry) {
            IContextData ctx = this.OwnerMenu?.CapturedContext ?? EmptyContext.Instance;
            this.CanExecute = entry.CanExecute(ctx);
        }
        else {
            this.CanExecute = false;
        }
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        this.UpdateCanExecute();
        base.OnLoaded(e);
    }

    protected override void OnClick(RoutedEventArgs e) {
        if (this.IsExecuting) {
            this.CanExecute = false;
            return;
        }

        this.IsExecuting = true;
        if (!(this.Entry is CustomContextEntry entry)) {
            base.OnClick(e);
            this.IsExecuting = false;
            this.UpdateCanExecute();
            return;
        }

        // disable execution while executing command
        this.CanExecute = false;
        base.OnClick(e);
        if (!this.DispatchCommand(entry)) {
            this.IsExecuting = false;
            this.CanExecute = true;
        }
    }

    private bool DispatchCommand(CustomContextEntry entry) {
        IContextData context = this.OwnerMenu?.CapturedContext ?? EmptyContext.Instance;
        if (!entry.CanExecute(context)) {
            return false;
        }

        Dispatcher.UIThread.Post(() => this.ExecuteCommand(entry, context), DispatcherPriority.Render);
        return true;
    }

    private async void ExecuteCommand(CustomContextEntry entry, IContextData context) {
        try {
            await entry.OnExecute(context);
        }
        catch (Exception e) {
            if (!Debugger.IsAttached) {
                await IMessageDialogService.Instance.ShowMessage(
                    "Error",
                    "An unexpected error occurred while processing command. " +
                    "FramePFX may or may not crash now, but you should probably restart and save just in case",
                    e.GetToString());
            }
        }
        finally {
            this.IsExecuting = false;
            this.UpdateCanExecute();
        }
    }
}