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
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Activities;

/// <summary>
/// A service for showing activities as foreground operations that stop the user doing anything until
/// the activity has completed, or until other special cases (i.e. external dialog cancellation)
/// </summary>
public interface IForegroundActivityService {
    static bool TryGetInstance([NotNullWhen(true)] out IForegroundActivityService? service) {
        return ApplicationPFX.Instance.ComponentStorage.TryGetComponent(out service);
    }

    /// <summary>
    /// Shows a dialog that closes when the activity completes.
    /// <para>
    /// <c>Deadlock warning:</c> when calling this method from within an activity, the <see cref="dialogCancellation"/>
    /// token MUST become cancelled at minimum when the activity becomes cancelled
    /// </para>
    /// </summary>
    /// <returns>
    /// A task that becomes completed once the dialog closes (either the activity completes or <see cref="dialogCancellation"/> becomes cancelled)
    /// </returns>
    /// <remarks>This method does not throw <see cref="OperationCanceledException"/></remarks>
    Task<WaitForActivityResult> WaitForActivity(WaitForActivityOptions options);

    /// <summary>
    /// Shows a dialog that closes when the activity completes.
    /// <para>
    /// <c>Deadlock warning:</c> when calling this method from within an activity, the <see cref="dialogCancellation"/>
    /// token MUST become cancelled at minimum when the activity becomes cancelled
    /// </para>
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
    /// <remarks>This method does not throw <see cref="OperationCanceledException"/></remarks>
    Task WaitForActivity(ITopLevel parentTopLevel, ActivityTask activity, CancellationToken dialogCancellation = default) {
        return this.WaitForActivity(new WaitForActivityOptions(parentTopLevel, activity, dialogCancellation));
    }

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
    /// <remarks>This method does not throw <see cref="OperationCanceledException"/></remarks>
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
    /// A cancellation token that forces the dialog to close and therefore this method to return,
    /// even if the activity is still running. Passing this as the activity's
    /// <see cref="ActivityTask.CancellationToken"/> is not necessary
    /// </param>
    /// <returns>
    /// A task that becomes completed either when the activity becomes completed before the dialog opens,
    /// or the dialog closes (either the activity completed or <see cref="dialogCancellation"/> becomes cancelled)
    /// </returns>
    /// <remarks>This method does not throw <see cref="OperationCanceledException"/></remarks>
    Task DelayedWaitForActivity(ITopLevel parentTopLevel, ActivityTask activity, int showDelay, CancellationToken dialogCancellation = default) {
        return this.DelayedWaitForActivity(new WaitForActivityOptions(parentTopLevel, activity, dialogCancellation), showDelay); 
    }

    /// <summary>
    /// Waits an amount of milliseconds before showing the dialog via <see cref="WaitForActivity"/>.
    /// This method is useful to prevent the dialog flashing onscreen for a tiny amount of time
    /// if the activity is likely to complete very quickly.
    /// </summary>
    /// <param name="options">The options</param>
    /// <param name="showDelay">
    /// The amount of milliseconds to wait before showing the dialog.
    /// When 0 is specified, this is the same as calling <see cref="WaitForActivity"/> directly
    /// </param>
    /// <returns>
    /// A task that becomes completed either when the activity becomes completed before the dialog opens,
    /// or the dialog closes (either the activity completed or <see cref="dialogCancellation"/> becomes cancelled)
    /// </returns>
    /// <remarks>This method does not throw <see cref="OperationCanceledException"/></remarks>
    async Task<WaitForActivityResult> DelayedWaitForActivity(WaitForActivityOptions options, int showDelay) {
        if (showDelay < 0)
            throw new ArgumentOutOfRangeException(nameof(showDelay), showDelay, "Show delay cannot be negative");

        if (showDelay > 0) {
            // Wait for either the activity to complete, or for the show delay to elapse
            await options.Activity.Task.TryWaitAsync(showDelay, options.DialogCancellation);
        }
        
        if (options.Activity.IsCompleted)
            return WaitForActivityResult.ActivityCompletedEarly;
        if (options.DialogCancellation.IsCancellationRequested)
            return WaitForActivityResult.DialogCancellationRequested;

        // Activity still running, and dialogCancellation is not cancelled
        return await this.WaitForActivity(options);
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
        return new SubActivity(activityTask.Progress, activityTask.Task, activityTask.myCts);
    }
}

public readonly struct WaitForActivityOptions(ITopLevel parentTopLevel, ActivityTask activity, CancellationToken dialogCancellation = default) {
    /// <summary>
    /// Gets the top level to show the dialog relative to
    /// </summary>
    public ITopLevel ParentTopLevel { get; } = parentTopLevel;

    /// <summary>
    /// Gets the activity we want to wait for
    /// </summary>
    public ActivityTask Activity { get; } = activity;

    /// <summary>
    /// Gets the cancellation token used to close the dialog forcefully. When cancelled, it will not attempt to cancel the activity.
    /// </summary>
    public CancellationToken DialogCancellation { get; } = dialogCancellation;

    /// <summary>
    /// Gets or sets if we should attempt to cancel the activity when the user (or OS) tries to close
    /// the dialog, or if we should just close the dialog forcefully.
    /// <para>
    /// Default value is true
    /// </para>
    /// </summary>
    public bool CancelActivityOnCloseRequest { get; init; } = true;

    /// <summary>
    /// Gets or sets if we should wait for the activity to become completed when the dialog
    /// is requested to close, or if we should just close the dialog.
    /// <para>
    /// Default value is true
    /// </para> 
    /// </summary>
    public bool WaitForActivityOnCloseRequest { get; init; } = true;

    /// <summary>
    /// Gets or sets if the window should show the option to close the dialog and let the activity becomes a
    /// background operation. Clicking the button would never actually cancel the activity
    /// </summary>
    public bool CanMinimizeIntoBackgroundActivity { get; init; } = false;

    public static void Validate(ref WaitForActivityOptions options) {
        if (options.Activity == null)
            throw new InvalidOperationException("Invalid " + nameof(WaitForActivityOptions));
    }
}

public readonly struct WaitForActivityResult {
    /// <summary>
    /// The result for when userDialog cancellation is requested
    /// </summary>
    public static WaitForActivityResult DialogCancellationRequested => default;

    /// <summary>
    /// The result for when an activity becomes completed early before the dialog is shown
    /// </summary>
    public static WaitForActivityResult ActivityCompletedEarly => default;

    /// <summary>
    /// The result for when the top level the dialog was supposed to be shown relative to was closed early
    /// </summary>
    public static WaitForActivityResult ParentWindowClosed => new WaitForActivityResult(false, true);
    
    /// <summary>
    /// Gets whether the user wanted to close the dialog to make the task run as a background operation.
    /// This can only be true when <see cref="WaitForActivityOptions.CanMinimizeIntoBackgroundActivity"/> is true
    /// </summary>
    public bool TransitionToBackground { get; }

    /// <summary>
    /// Gets whether the parent top-level window closed too early for the wait operation to be effective
    /// </summary>
    public bool ParentClosedEarly { get; }

    public WaitForActivityResult(bool transitionToBackground, bool parentClosedEarly) {
        this.TransitionToBackground = transitionToBackground;
        this.ParentClosedEarly = parentClosedEarly;
    }
}