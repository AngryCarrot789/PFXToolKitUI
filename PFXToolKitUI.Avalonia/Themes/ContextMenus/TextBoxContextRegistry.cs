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

using Avalonia;
using Avalonia.Controls;
using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.Avalonia.Themes.ContextMenus;

public static class TextBoxContextRegistry {
    public static readonly ContextRegistry Registry = new ContextRegistry("Text Box");
    public static readonly DataKey<TextBox> TextBoxDataKey = DataKey<TextBox>.Create(nameof(TextBox));

    static TextBoxContextRegistry() {
        FixedContextGroup group = Registry.GetFixedGroup("general");
        group.AddEntry(new TextBoxContextEntry("Undo", (t) => t.Undo(), TextBox.CanUndoProperty) {InputGestureText = "CTRL + Z"});
        group.AddEntry(new TextBoxContextEntry("Redo", (t) => t.Redo(), TextBox.CanRedoProperty) {InputGestureText = "CTRL + Y"});
        group.AddSeparator();
        group.AddEntry(new TextBoxContextEntry("Cut", (t) => t.Cut(), TextBox.CanCutProperty) {InputGestureText = "CTRL + X"});
        group.AddEntry(new TextBoxContextEntry("Copy", (t) => t.Copy(), TextBox.CanCopyProperty) {InputGestureText = "CTRL + C"});
        group.AddEntry(new TextBoxContextEntry("Paste", (t) => t.Paste(), TextBox.CanPasteProperty) {InputGestureText = "CTRL + V"});
        group.AddSeparator();
        group.AddEntry(new TextBoxContextEntry("Select All", (t) => t.SelectAll(), null) {InputGestureText = "CTRL + A"});
        group.AddSeparator();
        group.AddEntry(new TextBoxContextEntry("Clear Text", (t) => t.Clear(), null));
    }
}

public class TextBoxContextEntry : CustomContextEntry {
    private readonly Action<TextBox> invoke;
    private readonly DirectProperty<TextBox, bool>? canExecuteProperty;
    private TextBox? currTb;

    public TextBoxContextEntry(string header, Action<TextBox> invoke, DirectProperty<TextBox, bool>? canExecuteProperty) : base(header, null) {
        this.invoke = invoke;
        this.canExecuteProperty = canExecuteProperty;
        this.CapturedContextChanged += this.OnCapturedContextChanged;
    }

    private void OnCapturedContextChanged(BaseContextEntry sender, IContextData? oldCtx, IContextData? newCtx) {
        if (newCtx != null && TextBoxContextRegistry.TextBoxDataKey.TryGetContext(newCtx, out var newTextBox)) {
            if (!ReferenceEquals(this.currTb, newTextBox)) {
                this.OnTextBoxChanged(this.currTb, newTextBox);
                this.currTb = newTextBox;
            }
        }
        else if (this.currTb != null) {
            this.OnTextBoxChanged(this.currTb, null);
            this.currTb = null;
        }
        
        this.RaiseCanExecuteChanged();
    }

    private void OnTextBoxChanged(TextBox? oldTextBox, TextBox? newTextBox) {
        if (this.canExecuteProperty != null) {
            if (oldTextBox != null)
                oldTextBox.PropertyChanged -= this.OnTBPropertyChanged;
            if (newTextBox != null)
                newTextBox.PropertyChanged += this.OnTBPropertyChanged;
        }
    }

    private void OnTBPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
        if (e.Property == this.canExecuteProperty) {
            this.RaiseCanExecuteChanged();
        }
    }

    public override bool CanExecute(IContextData context) {
        return this.currTb != null && (this.canExecuteProperty == null || this.currTb.GetValue(this.canExecuteProperty));
    }

    public override Task OnExecute(IContextData context) {
        if (TextBoxContextRegistry.TextBoxDataKey.TryGetContext(context, out TextBox? textBox)) {
            this.invoke(textBox);
        }
        
        return Task.CompletedTask;
    }
}