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

using System.Diagnostics;

namespace PFXToolKitUI.Utils;

public static class TaskUtils {
    /// <summary>
    /// Creates a <see cref="CancellationTokenSource"/> that becomes cancelled when the task completes.
    /// If the task is already completed, it returns an already cancelled token
    /// </summary>
    /// <param name="task">The task</param>
    /// <param name="cancellationToken">
    /// A cancellation token that makes the returned <see cref="CancellationTokenSource"/> becomes cancelled
    /// </param>
    /// <returns>The cancellation token source</returns>
    public static CancellationTokenSource CreateCompletionSource(Task task, CancellationToken cancellationToken = default) {
        CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (!task.IsCompleted) {
            // We pass cts.Token so that the continuation is removed when cancelled, to prevent leaking cts
            _ = task.ContinueWith(static (_, state) => {
                try {
                    ((CancellationTokenSource) state!).Cancel();
                }
                catch (ObjectDisposedException) {
                    // ignored
                }
            }, state: cts, cts.Token);
        }

        if (task.IsCompleted) {
            cts.Cancel();
        }

        return cts;
    }

    /// <summary>
    /// Tries to wait for a task to become completed within an amount of time
    /// </summary>
    /// <param name="task">The task to wait for</param>
    /// <param name="timeoutMilliseconds">The timeout</param>
    /// <param name="cancellationToken">A cancellation token to additionally cancel the timeout</param>
    /// <returns>
    /// True if the task completed on its own, otherwise false because the 
    /// timeout elapsed or the cancellation token became cancelled
    /// </returns>
    /// <remarks>This method does not throw <see cref="OperationCanceledException"/></remarks>
    public static Task<bool> TryWaitAsync(this Task task, int timeoutMilliseconds, CancellationToken cancellationToken = default) {
        return task.TryWaitAsync(TimeSpan.FromMilliseconds(timeoutMilliseconds), cancellationToken);
    }

    /// <summary>
    /// Tries to wait for a task to become completed within an amount of time
    /// </summary>
    /// <param name="task">The task to wait for</param>
    /// <param name="timeout">The timeout</param>
    /// <param name="cancellationToken">A cancellation token to additionally cancel the timeout</param>
    /// <returns>
    /// True if the task completed on its own, otherwise false because the 
    /// timeout elapsed or the cancellation token became cancelled
    /// </returns>
    /// <remarks>This method does not throw <see cref="OperationCanceledException"/></remarks>
    public static Task<bool> TryWaitAsync(this Task task, TimeSpan timeout, CancellationToken cancellationToken = default) {
        if (task.IsCompleted || cancellationToken.IsCancellationRequested) {
            return Task.FromResult(task.IsCompleted);
        }
        
        return task.TryWaitAsyncImpl(timeout, cancellationToken);
    }

    private static async Task<bool> TryWaitAsyncImpl(this Task task, TimeSpan timeout, CancellationToken cancellationToken = default) {
        try {
            await task.WaitAsync(timeout, cancellationToken);

            Debug.Assert(task.IsCompleted);
            return task.IsCompleted;
        }
        catch (OperationCanceledException) {
            return false;
        }
        catch (TimeoutException) {
            return false;
        }
    }
}