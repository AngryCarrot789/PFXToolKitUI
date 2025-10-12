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

using PFXToolKitUI.Icons;

namespace PFXToolKitUI.AdvancedMenuService;

/// <summary>
/// A menu entry that executes a <see cref="CommandSystem.Command"/>
/// </summary>
public class CommandMenuEntry : BaseMenuEntry {
    public string CommandId { get; }

    public CommandMenuEntry(string commandId) {
        this.CommandId = commandId;
    }

    public CommandMenuEntry(string commandId, string displayName, string? description = null, Icon? icon = null) : base(displayName, description, icon) {
        this.CommandId = commandId;
    }
}