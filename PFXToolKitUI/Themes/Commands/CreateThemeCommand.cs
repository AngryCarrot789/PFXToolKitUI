// 
// Copyright (c) 2023-2025 REghZy
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

using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Configurations.UI;
using PFXToolKitUI.Services.UserInputs;
using PFXToolKitUI.Themes.Configurations;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Themes.Commands;

public class CreateThemeCommand : Command {
    public bool CopyKeys { get; }

    public CreateThemeCommand(bool copyKeys) {
        this.CopyKeys = copyKeys;
    }

    protected override async Task ExecuteCommandAsync(CommandEventArgs e) {
        if (!IThemeConfigurationTreeElement.TreeElementKey.TryGetContext(e.ContextData, out IThemeConfigurationTreeElement? tree)) {
            return;
        }

        if (!(tree.ThemeConfigurationPage is ThemeConfigurationPage page)) {
            return;
        }

        Theme? theme = page.TargetTheme;
        if (theme == null) {
            return;
        }

        ThemeManager manager = theme.ThemeManager;
        SingleUserInputInfo info = new SingleUserInputInfo(this.CopyKeys ? "Create a new clone of a theme" : "Create a new theme", "What do you want the theme to be called?", null) {
            Validate = (args) => {
                if (string.IsNullOrWhiteSpace(args.Input))
                    args.Errors.Add("Theme name cannot be an empty string or consist of only whitespaces");
                else if (manager.GetTheme(args.Input) != null)
                    args.Errors.Add("Theme already exists with this name");
            }
        };

        if (page.IsModified) {
            info.Footer = "Changes to the current theme will be reverted and applied to the new theme instead";
        }

        info.Text = TextIncrement.GetIncrementableString((x) => theme.ThemeManager.GetTheme(x) == null, theme.Name, out string? value) ? value : theme.Name;
        if (await IUserInputDialogService.Instance.ShowInputDialogAsync(info) != true) {
            return;
        }

        if (theme != page.TargetTheme) {
            // ... huh?
            return;
        }

        Theme newTheme = theme.ThemeManager.RegisterTheme(info.Text, theme, this.CopyKeys);
        page.ApplyAndRevertChanges(theme, newTheme);

        theme.ThemeManager.SetTheme(newTheme);
        page.TargetTheme = newTheme;
    }
}