// 
// Copyright (c) 2025-2025 REghZy
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

/// <summary>
/// A mechanism used to communicate with UI usages of commands.
/// <para>
/// For example, say Command A runs code such that it effectively changes the executability of Command B.
/// Normally, the UI command usage object should listen to events of the contextual objects and update
/// re-query the executability.
/// <para>
/// But if this is not possible, (e.g. the contextual object source code cannot be modified), then this class
/// can be used as a final resort; the UI command usage will listen to this class' events.
/// </para>
/// </para>
/// <para>
/// Ideally this class should not be used. Instead, use model events and listen to them in UI command usages.
/// This makes it more obvious as to why a command's executability is being re-evaluated, whereas with
/// this signal class, there is no reason other than 'just do it' (unless you search for all usages)
/// </para>
/// </summary>
public sealed class CommandUsageSignal {
    /// <summary>
    /// An event fired when the return value of <see cref="Command.CanExecute"/> may be different since it was last evaluated
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    public CommandUsageSignal() {
    }

    /// <summary>
    /// Raises the <see cref="CanExecuteChanged"/> event
    /// </summary>
    public void RaiseCanExecuteChanged() => this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Gets or creates a signal for the given context data
    /// </summary>
    public static CommandUsageSignal GetOrCreate(IMutableContextData context, DataKey<CommandUsageSignal> key) {
        if (!key.TryGetContext(context, out CommandUsageSignal? signal)) {
            context.Set(key, signal = new CommandUsageSignal());
        }

        return signal;
    }
}