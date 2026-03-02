// 
// Copyright (c) 2026-2026 REghZy
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

namespace PFXToolKitUI.Toolbars;

public class SimpleCommandToolBarButton : ToolBarButton {
    /// <summary>
    /// Gets the command ID that this button executes
    /// </summary>
    public string CommandId { get; }
    
    public SimpleCommandToolBarButton(IButtonElement button, string commandId) : base(button) {
        this.CommandId = commandId;
    }

    public override Executability CanExecute() {
        return CommandManager.Instance.CanExecute(this.CommandId, this.ContextData, null, null, true);
    }

    protected override async Task OnClickedAsync() {
        if (CommandManager.Instance.TryFindCommandById(this.CommandId, out Command? command)) {
            await CommandManager.Instance.Execute(command, this.ContextData, null, null, true);
        }
    }
}