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
using Avalonia.Input;
using Avalonia.Interactivity;

namespace PFXToolKitUI.Avalonia.Bindings;

public class TextBoxToEventPropertyBinder<TModel> : BaseAvaloniaPropertyToEventPropertyBinder<TModel> where TModel : class {
    private readonly Func<IBinder<TModel>, string> getText;
    private readonly Func<IBinder<TModel>, string, Task> updateModel;
    private bool isHandlingChangeModel;

    public TextBoxToEventPropertyBinder(string eventName, Func<IBinder<TModel>, string> getText, Func<IBinder<TModel>, string, Task> updateModel) : base(null, eventName) {
        this.getText = getText;
        this.updateModel = updateModel;
    }
    
    protected override void UpdateModelOverride() {
    }

    protected override void UpdateControlOverride() {
        if (this.IsFullyAttached) {
            string newValue = this.getText(this);
            ((TextBox) this.myControl!).Text = newValue;
            BugFix.TextBox_UpdateSelection((TextBox) this.myControl!);
        }
    }

    protected override void CheckAttachControl(Control control) {
        base.CheckAttachControl(control);
        if (!(control is TextBox))
            throw new InvalidOperationException("Attempt to attach non-textbox");
    }

    protected override void OnAttached() {
        base.OnAttached();
        TextBox tb = (TextBox) this.Control;
        tb.LostFocus += this.OnLostFocus;
        tb.KeyDown += this.OnKeyDown;
    }

    protected override void OnDetached() {
        base.OnDetached();
        TextBox tb = (TextBox) this.Control;
        tb.LostFocus -= this.OnLostFocus;
        tb.KeyDown -= this.OnKeyDown;
    }

    private void OnLostFocus(object? sender, RoutedEventArgs e) {
        this.UpdateControl();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e) {
        if (e.Key == Key.Escape) {
            this.UpdateControl();
        }
        else if (e.Key == Key.Enter) {
            if (!this.isHandlingChangeModel) {
                this.HandleChangeModel();
            }
        }
    }

    private async void HandleChangeModel() {
        try {
            if (!base.IsFullyAttached) {
                return;
            }
            
            this.isHandlingChangeModel = true;
            await this.updateModel(this, ((TextBox) this.myControl!).Text ?? "");
            this.UpdateControl();
        }
        catch (Exception e) {
            ApplicationPFX.Instance.Dispatcher.Post(() => throw e);
        }
        finally {
            this.isHandlingChangeModel = false;
        }
    }
}