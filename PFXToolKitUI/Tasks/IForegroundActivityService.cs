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
    /// <returns>
    /// A task that becomes completed once the dialog closes (either the activity completes or <see cref="dialogCancellation"/> becomes cancelled)
    /// </returns>
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
    /// <returns>
    /// A task that becomes completed once the dialog closes (either the activities complete or <see cref="dialogCancellation"/> becomes cancelled)
    /// </returns>
    Task WaitForSubActivities(ITopLevel parentTopLevel, IEnumerable<SubActivity> activities, CancellationToken dialogCancellation = default);

    /// <summary>
    /// Waits an amount of milliseconds before showing the dialog via <see cref="WaitForActivity"/>.
    /// This method is useful to prevent the dialog flashing onscreen for a tiny amount of time
    /// if the activity is likely to complete very quickly.
    /// </summary>
    /// <param name="parentTopLevel">The top-level that should be parented to the dialog</param>
    /// <param name="activity">The activity task to show the progress of</param>
    /// <param name="showDelay">
    /// The amount of milliseconds to wait before showing the dialog.
    /// When 0 is specified, this is the same as calling <see cref="WaitForActivity"/> directly
    /// </param>
    /// <param name="dialogCancellation">
    /// A cancellation token that forces the dialog to close and therefore
    /// this method to return, even if the activity is still running
    /// </param>
    /// <returns>
    /// A task that becomes completed either when the activity becomes completed before the dialog opens,
    /// or the dialog closes (either the activity completed or <see cref="dialogCancellation"/> becomes cancelled)
    /// </returns>
    async Task DelayedWaitForActivity(ITopLevel parentTopLevel, ActivityTask activity, int showDelay, CancellationToken dialogCancellation = default) {
        if (showDelay < 0)
            throw new ArgumentOutOfRangeException(nameof(showDelay), showDelay, "Show delay cannot be negative");
        
        if (showDelay > 0) {
            // Create a CTS that becomes cancelled when either
            // the activity is cancelled or dialogCancellation gets cancelled
            using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(activity.CancellationToken, dialogCancellation);

            try {
                // Wait for either the activity to complete, or for the show delay to elapse
                await Task.WhenAny(activity.Task, Task.Delay(showDelay, linkedCts.Token));
            }
            catch (OperationCanceledException) {
                // ignored
            }
            
            if (!activity.IsCompleted && !linkedCts.Token.IsCancellationRequested) {
                // Activity still running and no request to cancel it, and dialogCancellation is not cancelled, so we
                // show the dialog and blow it a kiss before this code fails somehow, I'm sure I missed something...
                await this.WaitForActivity(parentTopLevel, activity, dialogCancellation);
            }
        }
        else {
            await this.WaitForActivity(parentTopLevel, activity, dialogCancellation);
        }
    }
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