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

using System.Runtime.ExceptionServices;

namespace PFXToolKitUI;

// TODO: when adding support for mobile, we need to add this as an extension component to IDispatcher
// because mobile (at least android) does not support "Run loops", aka dispatcher frames.

// However windows does support them, not sure about mac and linux...

/// <summary>
/// An extension to <see cref="IDispatcher"/> that supports dispatcher frames
/// </summary>
public interface IDispatcherFrameManager {
    /// <summary>
    /// Pushes a new dispatcher frame that exits when the cancellation token is marked
    /// as cancelled. If already cancelled, then nothing happens.
    /// </summary>
    /// <param name="cancellationToken">The token that causes the frame to exit once cancelled</param>
    /// <exception cref="InvalidOperationException">The cancellation token is not cancellable</exception>
    void PushFrame(CancellationToken cancellationToken);

    /// <summary>
    /// Synchronously waits for the task to complete without stalling the dispatcher,
    /// by instead pushing a new dispatcher frame that exits once the task completes.
    /// <para>
    /// Note that this may cause ordering issues
    /// </para>
    /// </summary>
    /// <param name="task">The task to wait for completion</param>
    /// <exception cref="AggregateException">The task was faulted and <see cref="Task.Exception"/> was non-null</exception>
    void AwaitForCompletion(Task task) {
        if (!task.IsCompleted) {
            using CancellationTokenSource cts = new CancellationTokenSource();
            task.ContinueWith((t, theCts) => ((CancellationTokenSource) theCts!).Cancel(), cts, TaskContinuationOptions.ExecuteSynchronously);
            this.PushFrame(cts.Token);
        }

        AggregateException? exception = task.Exception;
        if (exception != null) {
            ExceptionDispatchInfo.Throw(exception);
        }
    }
}