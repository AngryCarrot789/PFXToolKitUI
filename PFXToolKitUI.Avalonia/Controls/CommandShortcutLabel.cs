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
using Avalonia.Controls.Primitives;
using PFXToolKitUI.Shortcuts;

namespace PFXToolKitUI.Avalonia.Controls;

public class CommandShortcutLabel : TemplatedControl {
    public static readonly StyledProperty<string?> NoShortcutTextProperty = AvaloniaProperty.Register<CommandShortcutLabel, string?>(nameof(NoShortcutText), defaultValue: "No Shortcuts");
    public static readonly StyledProperty<string?> CommandIdProperty = AvaloniaProperty.Register<CommandShortcutLabel, string?>(nameof(CommandId));
    public static readonly DirectProperty<CommandShortcutLabel, string?> CurrentGestureProperty = AvaloniaProperty.RegisterDirect<CommandShortcutLabel, string?>(nameof(CurrentGesture), o => o.CurrentGesture);

    /// <summary>
    /// Gets or sets the text displayed when no such shortcut exists for the command <see cref="CommandId"/>
    /// </summary>
    public string? NoShortcutText {
        get => this.GetValue(NoShortcutTextProperty);
        set => this.SetValue(NoShortcutTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the command id, which is used to query all available shortcuts
    /// </summary>
    public string? CommandId {
        get => this.GetValue(CommandIdProperty);
        set => this.SetValue(CommandIdProperty, value);
    }

    public string? CurrentGesture {
        get => field;
        private set => this.SetAndRaise(CurrentGestureProperty, ref field, value);
    }

    public CommandShortcutLabel() {
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnAttachedToVisualTree(e);
        this.UpdateText();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
        base.OnPropertyChanged(change);

        if (change.Property == CommandIdProperty || change.Property == NoShortcutTextProperty) {
            this.UpdateText();
        }
    }

    private void UpdateText() {
        if (!KeymapUtils.TryGetStringForCommandId(this.CommandId, out string? gesture)) {
            gesture = this.NoShortcutText;
        }

        this.CurrentGesture = gesture;
    }
}