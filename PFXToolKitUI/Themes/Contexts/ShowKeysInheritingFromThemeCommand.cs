// 
// Copyright (c) 2023-2025 REghZy
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

using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Themes.Configurations;

namespace PFXToolKitUI.Themes.Contexts;

public class ShowKeysInheritingFromThemeCommand : Command {
    protected override Executability CanExecuteCore(CommandEventArgs e) {
        if (!ThemeContextRegistry.ThemeTreeEntryKey.IsPresent(e.ContextData))
            return Executability.Invalid;
        if (!ThemeContextRegistry.ThemeConfigurationPageKey.TryGetContext(e.ContextData, out ThemeConfigurationPage? page))
            return Executability.Invalid;

        return page.TargetTheme == null ? Executability.ValidButCannotExecute : Executability.Valid;
    }

    protected override async Task ExecuteCommandAsync(CommandEventArgs e) {
        if (!ThemeContextRegistry.ThemeTreeEntryKey.TryGetContext(e.ContextData, out IThemeTreeEntry? entry)) {
            return;
        }

        if (!ThemeContextRegistry.ThemeConfigurationPageKey.TryGetContext(e.ContextData, out ThemeConfigurationPage? page)) {
            return;
        }

        if (!(entry is ThemeConfigEntry cfgEntry)) {
            return;
        }

        if (page.TargetTheme == null) {
            return;
        }

        HashSet<string> keys = new HashSet<string>(16);
        page.TargetTheme.CollectKeysInheritedBy(cfgEntry.ThemeKey, keys);
        if (keys.Count > 0) {
            await IMessageDialogService.Instance.ShowMessage("Keys", $"Theme keys that inherit from '{cfgEntry.ThemeKey}'", string.Join(Environment.NewLine, keys), defaultButton:MessageBoxResult.OK);
        }
        else {
            await IMessageDialogService.Instance.ShowMessage("No keys", $"No keys inherit from '{cfgEntry.ThemeKey}'", defaultButton:MessageBoxResult.OK);
        }
    }
}