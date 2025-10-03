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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.AdvancedMenuService;

public class AdvancedCustomMenuItem : AdvancedMenuItem {
    private bool canExecute;
    private TextBlock? InputGestureTextBlock;
    private readonly IBinder<CustomContextEntry> gestureBinder = new EventUpdateBinder<CustomContextEntry>(nameof(CustomContextEntry.InputGestureTextChanged), b => b.Control.SetValue(TextBlock.TextProperty, !string.IsNullOrWhiteSpace(b.Model.InputGestureText) ? b.Model.InputGestureText : null));

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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.InputGestureTextBlock = e.NameScope.GetTemplateChild<TextBlock>("PART_InputGestureText");
        this.gestureBinder.AttachControl(this.InputGestureTextBlock);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnAttachedToVisualTree(e);
        this.gestureBinder.AttachModel(this.Entry!);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnDetachedFromVisualTree(e);
        this.gestureBinder.DetachModel();
    }

    public override void UpdateCanExecute() {
        if (this.IsLoaded) {
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
    }

    protected override void OnLoadedOverride(RoutedEventArgs e) {
        base.OnLoadedOverride(e);
        this.UpdateCanExecute();
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

        ApplicationPFX.Instance.Dispatcher.Post(ExecuteContextEntry, DispatchPriority.Render);
        return true;

        async void ExecuteContextEntry() {
            try {
                await entry.OnExecute(context);
            }
            catch (Exception exception) when (!Debugger.IsAttached) {
                await LogExceptionHelper.ShowMessageAndPrintToLogs("Internal Action Error", exception);
            }
            finally {
                this.IsExecuting = false;
                this.UpdateCanExecute();
            }
        }
    }
}