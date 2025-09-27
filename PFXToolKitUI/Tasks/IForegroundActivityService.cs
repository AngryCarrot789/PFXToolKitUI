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
/// A service for showing activities as foreground operations that stop the user doing anything until
/// the activity has completed, or until other special cases (i.e. external dialog cancellation)
/// </summary>
public interface IForegroundActivityService {
    static bool TryGetInstance([NotNullWhen(true)] out IForegroundActivityService? service) {
        return ApplicationPFX.Instance.ComponentStorage.TryGetComponent(out service);
    }

    /// <summary>
    /// Shows a dialog that closes when the activity completes
    /// </summary>
    /// <param name="parentTopLevel">The top-level that should be parented to the dialog</param>
    /// <param name="activity">The activity task to show the progress of</param>
    /// <param name="dialogCancellation">
    /// A cancellation token that forces the dialog to close and therefore
    /// this method to return, even if the activity is still running
    /// </param>
    /// <returns>A task that becomes completed once the activity completes and the dialog closes</returns>
    Task WaitForActivity(ITopLevel parentTopLevel, ActivityTask activity, CancellationToken dialogCancellation = default);

    /// <summary>
    /// Shows a dialog that closes when all the sub-activities all complete
    /// </summary>
    /// <param name="parentTopLevel"></param>
    /// <param name="activities">
    /// A list of <see cref="SubActivity"/> structures that contain the information
    /// required to present the progress and optionally cancel the operation
    /// </param>
    /// <param name="dialogCancellation">
    /// A cancellation token that forces the dialog to close and therefore
    /// this method to return, even if the task is still running
    /// </param>
    /// <returns>A task that becomes completed once all the sub-activities complete and the dialog closes</returns>
    Task WaitForSubActivities(ITopLevel parentTopLevel, IEnumerable<SubActivity> activities, CancellationToken dialogCancellation = default);
}

/// <summary>
/// Represents a "sub-activity", which represents a lightweight version of <see cref="ActivityTask"/> 
/// </summary>
public readonly struct SubActivity(IActivityProgress progress, Task task, CancellationTokenSource? cancellation) {
    /// <summary>
    /// Gets the progression object for tracking progress of the activity
    /// </summary>
    public IActivityProgress Progress { get; } = progress;

    /// <summary>
    /// Gets the task that represents the activity's operation
    /// </summary>
    public Task Task { get; } = task;

    /// <summary>
    /// Gets the optional cancellation token source that can be used to
    /// request <see cref="Task"/> to stop as soon as possible
    /// </summary>
    public CancellationTokenSource? Cancellation { get; } = cancellation;
    
    /// <summary>
    /// Creates a sub-activity from an <see cref="ActivityTask"/>
    /// </summary>
    /// <param name="activityTask">The activity task</param>
    /// <returns>A new sub-activity</returns>
    /// <remarks>
    /// Note that the <see cref="ActivityTask"/> will not be notified that it is being shown in a dialog when
    /// using this method for <see cref="IForegroundActivityService.WaitForSubActivities"/>
    /// </remarks>
    public static SubActivity FromActivity(ActivityTask activityTask) {
        return new SubActivity(activityTask.Progress, activityTask.Task, activityTask.cancellationTokenSource);
    }
}