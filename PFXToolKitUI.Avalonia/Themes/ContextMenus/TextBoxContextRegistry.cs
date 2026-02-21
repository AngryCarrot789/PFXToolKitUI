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
using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.Avalonia.Themes.ContextMenus;

public static class TextBoxContextRegistry {
    public static readonly ContextRegistry Registry = new ContextRegistry("Text Box");

    /// <summary>
    /// The data key used to identify the text box that uses <see cref="TextBoxContextRegistry"/> for its context menu
    /// </summary>
    public static readonly DataKey<TextBox> TextBoxDataKey = DataKeys.Create<TextBox>(nameof(TextBox));

    static TextBoxContextRegistry() {
        FixedWeightedMenuEntryGroup group = Registry.GetFixedGroup("general");
        group.AddEntry(new TextBoxMenuEntry("Undo", (t) => t.Undo(), TextBox.CanUndoProperty) { InputGestureText = "CTRL+Z" });
        group.AddEntry(new TextBoxMenuEntry("Redo", (t) => t.Redo(), TextBox.CanRedoProperty) { InputGestureText = "CTRL+Y or CTRL+SHIFT+Z" });
        group.AddSeparator();
        group.AddEntry(new TextBoxMenuEntry("Cut", (t) => t.Cut(), TextBox.CanCutProperty) { InputGestureText = "CTRL+X" });
        group.AddEntry(new TextBoxMenuEntry("Copy", (t) => t.Copy(), TextBox.CanCopyProperty) { InputGestureText = "CTRL+C" });
        group.AddEntry(new TextBoxMenuEntry("Paste", (t) => t.Paste(), TextBox.CanPasteProperty) { InputGestureText = "CTRL+V" });
        group.AddSeparator();
        group.AddEntry(new TextBoxMenuEntry("Select All", (t) => t.SelectAll(), null) { InputGestureText = "CTRL+A" });
        group.AddSeparator();
        group.AddEntry(new TextBoxMenuEntry("Clear Text", (t) => t.Clear(), null));
    }
}

public class TextBoxMenuEntry : CustomMenuEntry {
    private readonly Action<TextBox> invoke;
    private readonly DirectProperty<TextBox, bool>? canExecuteProperty;
    private TextBox? currentTextBox;

    public TextBoxMenuEntry(string header, Action<TextBox> invoke, DirectProperty<TextBox, bool>? canExecuteProperty) : base(header, null) {
        this.invoke = invoke;
        this.canExecuteProperty = canExecuteProperty;
    }

    protected override void OnCapturedContextChanged(IContextData? oldContext, IContextData? newContext) {
        base.OnCapturedContextChanged(oldContext, newContext);
        this.OnContextChangedHelper(TextBoxContextRegistry.TextBoxDataKey, ref this.currentTextBox, static (@this, e) => {
            if (@this.canExecuteProperty != null) {
                if (e.OldValue != null)
                    e.OldValue.PropertyChanged -= @this.OnTextBoxPropertyChanged;
                if (e.NewValue != null)
                    e.NewValue.PropertyChanged += @this.OnTextBoxPropertyChanged;
            }
        });

        this.RaiseCanExecuteChanged();
    }

    private void OnTextBoxPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
        if (e.Property == this.canExecuteProperty) {
            this.RaiseCanExecuteChanged();
        }
    }

    public override bool CanExecute(IContextData context) {
        return this.currentTextBox != null && (this.canExecuteProperty == null || this.currentTextBox.GetValue(this.canExecuteProperty));
    }

    public override Task OnExecute(IContextData context) {
        if (TextBoxContextRegistry.TextBoxDataKey.TryGetContext(context, out TextBox? textBox)) {
            this.invoke(textBox);
        }

        return Task.CompletedTask;
    }
}