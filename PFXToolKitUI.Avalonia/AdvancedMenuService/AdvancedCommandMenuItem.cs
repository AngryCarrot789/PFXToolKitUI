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

using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Shortcuts.Converters;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.AdvancedMenuService;

public class AdvancedCommandMenuItem : AdvancedMenuItem {
    private bool canExecute;
    private TextBlock? InputGestureTextBlock;

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

    public new CommandContextEntry? Entry => (CommandContextEntry?) base.Entry;

    protected override bool IsEnabledCore => base.IsEnabledCore && this.CanExecute;

    protected ContextRegistry? SourceContextMenu => this.OwnerMenu is AdvancedContextMenu menu ? menu.MyContextRegistry : null;

    public AdvancedCommandMenuItem() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.InputGestureTextBlock = e.NameScope.GetTemplateChild<TextBlock>("PART_InputGestureText");
        this.UpdateInputGestureText();
    }

    protected override void OnLoadedOverride(RoutedEventArgs e) {
        this.UpdateCanExecute();
        base.OnLoadedOverride(e);
        this.UpdateInputGestureText();
    }

    private void UpdateInputGestureText() {
        if (this.InputGestureTextBlock == null) {
            return;
        }

        CommandContextEntry? entry = this.Entry;
        if (entry != null && CommandManager.Instance.GetCommandById(entry.CommandId) != null) {
            if (CommandIdToGestureConverter.CommandIdToGesture(entry.CommandId, out string? value)) {
                this.InputGestureTextBlock.Text = value;
            }
        }
    }

    public override void UpdateCanExecute() {
        base.UpdateCanExecute();
        if (!this.IsLoaded) {
            return;
        }

        if (this.IsExecuting) {
            this.CanExecute = false;
        }
        else {
            IContextData? ctx = this.OwnerMenu?.CapturedContext;
            string? cmdId = this.Entry?.CommandId;
            Executability state = !string.IsNullOrWhiteSpace(cmdId) && ctx != null
                ? CommandManager.Instance.CanExecute(cmdId, ctx, null, this.SourceContextMenu)
                : Executability.Invalid;
            this.CanExecute = state == Executability.Valid;
            this.IsVisible = state != Executability.Invalid;
        }
    }

    protected override void OnClick(RoutedEventArgs e) {
        if (this.IsExecuting) {
            this.CanExecute = false;
            return;
        }

        this.IsExecuting = true;
        Command? command = CommandManager.Instance.GetCommandById(this.Entry?.CommandId);
        IContextData? context;
        if (command == null || (context = this.OwnerMenu?.CapturedContext) == null) {
            base.OnClick(e);
            this.IsExecuting = false;
            this.UpdateCanExecute();
            return;
        }

        // disable execution while executing command
        this.CanExecute = false;
        base.OnClick(e);

        ContextRegistry? sourceMenu = this.SourceContextMenu;
        ApplicationPFX.Instance.Dispatcher.Post(ExecuteContextCommand, DispatchPriority.Render);
        return;

        async void ExecuteContextCommand() {
            try {
                await CommandManager.Instance.Execute(command, context, null, sourceMenu);
            }
            catch (Exception exception) when (!Debugger.IsAttached) {
                await LogExceptionHelper.ShowMessageAndPrintToLogs("Command Error", exception);
            }
            finally {
                this.IsExecuting = false;
                this.UpdateCanExecute();
            }
        }
    }
}