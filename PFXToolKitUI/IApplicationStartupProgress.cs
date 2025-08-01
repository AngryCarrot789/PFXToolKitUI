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

using PFXToolKitUI.Tasks;

namespace PFXToolKitUI;

public interface IApplicationStartupProgress {
    /// <summary>
    /// Gets or sets the current action
    /// </summary>
    string? ActionText { get; set; }

    /// <summary>
    /// Gets the completion state progress bar
    /// </summary>
    CompletionState CompletionState { get; }

    /// <summary>
    /// Updates the action (if non-null) and sets the current progress (if non-null)
    /// and then awaits <see cref="WaitForRender"/>
    /// </summary>
    /// <param name="action">New <see cref="ActionText"/> if non-null</param>
    /// <param name="newProgress">Value passed to <see cref="Tasks.CompletionState.SetProgress"/> if non-null</param>
    /// <returns>A task completed once rendered</returns>
    Task ProgressAndWaitForRender(string? action, double? newProgress = null);

    /// <summary>
    /// Returns a task that completes once the UI has been rendered with the current <see cref="ActionText"/> and <see cref="CompletionState"/>
    /// </summary>
    /// <returns></returns>
    Task WaitForRender();
}

public class EmptyApplicationStartupProgress : IApplicationStartupProgress {
    public string? ActionText { get; set; }

    public CompletionState CompletionState { get; } = new SimpleCompletionState();
    public Task ProgressAndWaitForRender(string? action, double? newProgress) => Task.CompletedTask;
    public Task WaitForRender() => Task.CompletedTask;
}