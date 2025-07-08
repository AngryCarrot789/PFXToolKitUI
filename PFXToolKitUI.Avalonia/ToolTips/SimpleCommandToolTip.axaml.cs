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
using PFXToolKitUI.Avalonia.Shortcuts.Converters;
using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.Avalonia.ToolTips;

public partial class SimpleCommandToolTip : UserControl, IToolTipControl {
    public static readonly AttachedProperty<string?> TextProperty = AvaloniaProperty.RegisterAttached<SimpleCommandToolTip, Control, string?>("Text");
    public static readonly AttachedProperty<string?> CommandIdProperty = AvaloniaProperty.RegisterAttached<SimpleCommandToolTip, Control, string?>("CommandId");

    public SimpleCommandToolTip() {
        this.InitializeComponent();
    }

    static SimpleCommandToolTip() {
        TextProperty.Changed.AddClassHandler<Control, string?>((s, e) => OnTextOrCommandIdChanged(s, e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        CommandIdProperty.Changed.AddClassHandler<Control, string?>((s, e) => OnTextOrCommandIdChanged(s, e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    private static void OnTextOrCommandIdChanged(Control control, string? oldValue, string? newValue) {
        ToolTipEx.SetTipType(control, GetText(control) != null || !string.IsNullOrWhiteSpace(GetCommandId(control)) ? typeof(SimpleCommandToolTip) : null);
    }

    public static void SetText(Control obj, string? value) => obj.SetValue(TextProperty, value);
    public static string? GetText(Control obj) => obj.GetValue(TextProperty);
    public static void SetCommandId(Control obj, string? value) => obj.SetValue(CommandIdProperty, value);
    public static string? GetCommandId(Control obj) => obj.GetValue(CommandIdProperty);
    
    public void OnOpened(Control owner, IContextData data) {
        this.PART_Text.Text = GetText(owner);
        if (CommandIdToGestureConverter.CommandIdToGesture(GetCommandId(owner), out string? gesture)) {
            this.PART_ShortcutBackground.IsVisible = true;
            this.PART_Shortcut.Text = gesture;
        }
        else {
            this.PART_ShortcutBackground.IsVisible = false;
        }
    }

    public void OnClosed(Control owner) {
        this.PART_Text.Text = "";
        this.PART_ShortcutBackground.IsVisible = false;
    }
}