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

using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Configurations.UI;

namespace PFXToolKitUI.Configurations.Commands;

public class CollapseThemeConfigTreeCommand : Command {
    protected override Executability CanExecuteCore(CommandEventArgs e) {
        return IThemeConfigurationTreeElement.TreeElementKey.GetExecutabilityForPresence(e.ContextData);
    }

    protected override Task ExecuteCommandAsync(CommandEventArgs e) {
        if (IThemeConfigurationTreeElement.TreeElementKey.TryGetContext(e.ContextData, out IThemeConfigurationTreeElement? tree)) {
            tree.CollapseAll();
        }

        return Task.CompletedTask;
    }
}