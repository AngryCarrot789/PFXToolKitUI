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
using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.CommandSystem;

public delegate void CommandUsageIconChangedEventHandler(ICommandUsage usage, Icon? oldIcon, Icon? newIcon);

/// <summary>
/// An object that is associated with, typically, a single UI control, and manages specific behaviours in relation to a command
/// </summary>
public interface ICommandUsage {
    /// <summary>
    /// Gets the target command ID for this usage instance. This is not null, not empty and
    /// does not consist of only whitespaces; it's a fully valid command ID
    /// </summary>
    string CommandId { get; }

    /// <summary>
    /// Gets the context data that is current available. May be empty, but will not be null
    /// </summary>
    IContextData ContextData { get; }

    /// <summary>
    /// Gets or sets the icon this command should display
    /// </summary>
    Icon? Icon { get; set; }

    /// <summary>
    /// Fired when the <see cref="Icon"/> changes
    /// </summary>
    event CommandUsageIconChangedEventHandler? IconChanged;

    /// <summary>
    /// Triggers an update on this usage. This may cause a button (that executes the command) to
    /// become enabled or disabled based on the available information in our <see cref="ContextData"/>
    /// </summary>
    void UpdateCanExecuteLater();

    /// <summary>
    /// Updates the UI component based on whether the command can currently be executed
    /// </summary>
    void UpdateCanExecute();
}