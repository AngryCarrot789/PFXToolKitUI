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

using System.Diagnostics.CodeAnalysis;
using PFXToolKitUI.Interactivity.Windowing;

namespace PFXToolKitUI.Tasks;

/// <summary>
/// A service for 
/// </summary>
public interface IForegroundActivityService {
    public static bool TryGetInstance([NotNullWhen(true)] out IForegroundActivityService? service) {
        return ApplicationPFX.Instance.ComponentStorage.TryGetComponent(out service);
    }

    /// <summary>
    /// Shows a dialog that closes when the activity completes
    /// </summary>
    /// <param name="topLevel">The window to show the dialog in</param>
    /// <param name="task">The task to show the progress of</param>
    /// <param name="dialogCancellation">
    ///     A cancellation token that forces the dialog to close and therefore
    ///     this method to return, even if the task is still running
    /// </param>
    /// <returns>A task that becomes completed once the activity completes and the dialog closes</returns>
    Task ShowActivity(ITopLevel topLevel, ActivityTask task, CancellationToken dialogCancellation = default);

    /// <summary>
    /// Shows a dialog that closes when the activity completes
    /// </summary>
    /// <param name="topLevel"></param>
    /// <param name="progressions">
    ///     A list of <see cref="IActivityProgress"/> objects with an associated
    ///     <see cref="TaskCompletionSource"/> that represents its completion state
    /// </param>
    /// <param name="dialogCancellation">
    ///     A cancellation token that forces the dialog to close and therefore
    ///     this method to return, even if the task is still running
    /// </param>
    /// <param name="task">The task</param>
    /// <returns>A task that becomes completed once the activity completes and the dialog closes</returns>
    Task ShowMultipleProgressions(ITopLevel topLevel, IEnumerable<(IActivityProgress Progress, Task Task)> progressions, CancellationToken dialogCancellation = default);
}