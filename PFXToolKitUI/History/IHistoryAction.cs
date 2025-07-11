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

namespace PFXToolKitUI.History;

public interface IHistoryAction {
    /// <summary>
    /// Undoes this action
    /// </summary>
    /// <returns>True if the undo was successful, otherwise false, meaning this action stays at the top of the undo stack</returns>
    Task<bool> Undo();

    /// <summary>
    /// Redoes this action
    /// </summary>
    /// <returns>True if the redo was successful, otherwise false, meaning this action stays at the top of the redo stack</returns>
    Task<bool> Redo();

    /// <summary>
    /// Disposes this history action. This is called when it is no longer reachable/deleted (e.g. it was undone then another action was executed, meaning the history got cleared)
    /// </summary>
    void Dispose();
}