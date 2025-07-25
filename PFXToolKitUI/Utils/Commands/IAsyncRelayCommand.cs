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

namespace PFXToolKitUI.Utils.Commands;

public interface IAsyncRelayCommand : IRelayCommand {
    /// <summary>
    /// Gets whether this command is currently executing a task
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Executes this command asynchronously, if it is not already running
    /// </summary>
    /// <param name="parameter">A parameter to pass to the command</param>
    /// <returns>The command's work</returns>
    Task ExecuteAsync(object? parameter);

    /// <summary>
    /// Executes this command if it is not already running. If it's running, this function returns false, otherwise true
    /// </summary>
    /// <param name="parameter">A parameter to pass to the command</param>
    /// <returns>The command's work</returns>
    Task<bool> TryExecuteAsync(object? parameter);
}