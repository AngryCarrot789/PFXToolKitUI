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
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Configurations.Pages;
using PFXToolKitUI.Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Shortcuts.Trees;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Configurations.Shortcuts;
using PFXToolKitUI.Configurations.UI;
using PFXToolKitUI.Shortcuts;

namespace PFXToolKitUI.Avalonia.Shortcuts.Configurations;

public class ShortcutEditorConfigurationPageControl : BaseConfigurationPageControl {
    private ShortcutTreeView? PART_ShortcutTree;

    public ShortcutEditorConfigurationPageControl() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_ShortcutTree = e.NameScope.GetTemplateChild<ShortcutTreeView>("PART_ShortcutTree");
        DataManager.GetContextData(this).Set(IShortcutTreeElement.TreeElementKey, this.PART_ShortcutTree);

        if (e.NameScope.TryGetTemplateChild("PART_DemoHyperlink", out HyperlinkButton? hyperlink)) {
            hyperlink.Click += this.OnHyperlinkClicked;
        }
    }

    private void OnHyperlinkClicked(object? sender, RoutedEventArgs e) {
        if (((HyperlinkButton) sender!).Content is string text) {
            ShortcutGroupEntry? target = this.PART_ShortcutTree?.RootEntry?.GetGroupByPath(text);
            if (target != null) {
                this.PART_ShortcutTree!.ExpandTo(target);
            }
        }
    }

    public override void OnConnected() {
        base.OnConnected();
        this.PART_ShortcutTree!.RootEntry = ((ShortcutEditorConfigurationPage) this.Page!).RootGroupEntry;
    }

    public override void OnDisconnected() {
        base.OnDisconnected();
        this.PART_ShortcutTree!.RootEntry = null;
    }
}