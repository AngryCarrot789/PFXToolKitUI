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

namespace PFXToolKitUI.CommandSystem;

/// <summary>
/// An enum that represents the executable state of a command based on available context.
/// <para>
/// For example, given a command <c>DeleteSelectedObjectsCommand</c> that can delete selected objects of some controller object, e.g., <c>ObjectManager</c>
/// </para>
/// <para>
/// If the context does not contain the <c>ObjectManager</c> then the executability is <see cref="Invalid"/>.
/// </para>
/// <para>
/// If there IS an <c>ObjectManager</c>, but nothing is selected or some operation is going on that has
/// disabled selection, then <see cref="ValidButCannotExecute"/> can be used.
/// </para>
/// <para>
/// If there is an <c>ObjectManager</c> and selection is modifiable, then <see cref="Valid"/> is used, meaning
/// <see cref="Command.ExecuteCommandAsync"/> will most likely result in work being done (unless there's an issue
/// in the command implementation, or there's no overridden implementation, or the state of the contextual
/// data changes since <see cref="Command.CanExecuteCore"/> was invoked)
/// </para>
/// </summary>
public enum Executability {
    /// <summary>
    /// The context does not contain the relevant data for the command to execute.
    /// This might also be used if a targeted command does not exist
    /// </summary>
    Invalid,

    /// <summary>
    /// The context contains the correct information but the state of the context data is
    /// not "appropriate" (e.g. deleted an already deleted object), therefore the command cannot run
    /// </summary>
    ValidButCannotExecute,

    /// <summary>
    /// The context contains the correct information and the command is fully executable.
    /// This is the default value returned by <see cref="Command.CanExecuteCore"/>.
    /// </summary>
    Valid
}