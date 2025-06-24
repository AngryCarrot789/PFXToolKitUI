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

using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.CommandSystem;

public abstract class ToggleCommand : Command {
    public static readonly DataKey<bool> IsToggledKey = DataKey<bool>.Create("Toggled");

    /// <summary>
    /// Gets whether the given event context is toggled or not
    /// </summary>
    /// <param name="e">The command event args, containing info about the current context</param>
    /// <returns>A nullable boolean that states the toggle state, or null if no toggle state is present</returns>
    public virtual bool? GetIsToggled(CommandEventArgs e) {
        return IsToggledKey.TryGetContext(e.ContextData, out bool value) ? (bool?) value : null;
    }

    protected override Task ExecuteCommandAsync(CommandEventArgs e) {
        bool? t = this.GetIsToggled(e);
        return t.HasValue ? this.OnToggled(e, t.Value) : this.ExecuteNoToggle(e);
    }

    /// <summary>
    /// Called when the command is executed with the given toggle state
    /// </summary>
    /// <param name="e">The command event args, containing info about the current context</param>
    /// <param name="isToggled">The toggle state of whatever called the command</param>
    /// <returns>Whether the command was executed successfully</returns>
    protected abstract Task OnToggled(CommandEventArgs e, bool isToggled);

    /// <summary>
    /// Called when the command was executed without any toggle info. This can be
    /// used to, for example, invert a known toggle state
    /// </summary>
    /// <param name="e">The command event args, containing info about the current context</param>
    /// <returns>Whether the command was executed successfully</returns>
    protected abstract Task ExecuteNoToggle(CommandEventArgs e);

    protected override Executability CanExecuteCore(CommandEventArgs e) {
        bool? result = this.GetIsToggled(e);
        return result.HasValue ? this.CanExecute(e, result.Value) : this.CanExecuteNoToggle(e);
    }

    protected virtual Executability CanExecute(CommandEventArgs e, bool isToggled) {
        return Executability.Valid;
    }

    protected virtual Executability CanExecuteNoToggle(CommandEventArgs e) {
        return this.CanExecute(e, false);
    }
}