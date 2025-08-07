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

using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using PFXToolKitUI.Avalonia.ToolTips;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Shortcuts;

namespace PFXToolKitUI.Avalonia.Shortcuts.Trees;

public partial class ShotcutTreeViewItemToolTip : UserControl, IToolTipControl {
    public ShotcutTreeViewItemToolTip() {
        this.InitializeComponent();
    }

    public void OnOpened(Control owner, IContextData data) {
        bool insertSpacing = false;

        if (!IKeyMapEntry.DataKey.TryGetContext(data, out IKeyMapEntry? entry)) {
            this.PART_TextBlock.Inlines?.Clear();
            return;
        }

        InlineCollection inlines = this.PART_TextBlock.Inlines ??= new InlineCollection();
        if (entry is ShortcutEntry shortcut && !string.IsNullOrWhiteSpace(shortcut.CommandId)) {
            inlines.Add(new Run("Target Command ID") { FontSize = 16, FontWeight = FontWeight.Bold, BaselineAlignment = BaselineAlignment.Center });
            inlines.Add(new LineBreak());

            if (CommandManager.Instance.GetCommandById(shortcut.CommandId) != null) {
                inlines.Add(new Run(shortcut.CommandId) { FontWeight = FontWeight.Normal });
            }
            else {
                inlines.Add(new Run(shortcut.CommandId) { TextDecorations = TextDecorations.Strikethrough, FontStyle = FontStyle.Italic, FontWeight = FontWeight.Normal });
                inlines.Add(new Run(" (does not exist)"));
            }

            insertSpacing = true;
        }

        if (!string.IsNullOrWhiteSpace(entry.Description)) {
            if (insertSpacing) {
                inlines.Add(new LineBreak());
                inlines.Add(new LineBreak());
            }

            inlines.Add(new Run(entry.Description) { BaselineAlignment = BaselineAlignment.Center });
        }

        if (inlines.Count < 1) {
            ApplicationPFX.Instance.Dispatcher.Post(() => ToolTip.SetIsOpen(owner, false), DispatchPriority.Loaded);
        }
    }

    public void OnClosed(Control owner) {
        this.PART_TextBlock.Inlines?.Clear();
    }
}