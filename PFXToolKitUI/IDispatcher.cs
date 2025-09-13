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

/// <summary>
/// Provides a mechanism for invoking actions on a specific thread
/// </summary>
public interface IDispatcher {
    /// <summary>
    /// Determines whether the calling thread is the thread associated with this dispatcher
    /// </summary>
    bool CheckAccess();

    /// <summary>
    /// Throws an exception if the calling thread is not the thread associated with this dispatcher
    /// </summary>
    void VerifyAccess();

    /// <summary>
    /// Synchronously executes the given function on the dispatcher thread, or dispatches its execution on the dispatcher thread if we are not
    /// currently on it. This effectively blocks the current thread until the <see cref="Action"/> returns
    /// </summary>
    /// <param name="action">The function to execute on the dispatcher thread</param>
    /// <param name="priority">The priority of the dispatch</param>
    void Invoke(Action action, DispatchPriority priority = DispatchPriority.Send);

    /// <summary>
    /// The same as <see cref="Invoke"/> but allows a return value
    /// </summary>
    /// <param name="function">The function to execute on the dispatcher thread</param>
    /// <param name="priority">The priority of the dispatch</param>
    /// <typeparam name="TResult">The return value for the function</typeparam>
    /// <returns>The return value of the parameter '<see cref="function"/>'</returns>
    T Invoke<T>(Func<T> function, DispatchPriority priority = DispatchPriority.Send);

    /// <summary>
    /// Asynchronously executes the given function on the dispatcher thread, or dispatches its execution on the dispatcher thread
    /// if we are not currently on it. This is the best way to execute a function on the dispatcher thread asynchronously
    /// </summary>
    /// <param name="action">The function to execute on the dispatcher thread</param>
    /// <param name="priority">The priority of the dispatch</param>
    /// <param name="token"></param>
    /// <returns>A task that can be awaited, which is completed once the function returns on the dispatcher thread</returns>
    Task InvokeAsync(Action action, DispatchPriority priority = DispatchPriority.Normal, CancellationToken token = default);

    /// <summary>
    /// The same as <see cref="InvokeAsync"/> but allows a return value
    /// </summary>
    /// <param name="function">The function to execute on the dispatcher thread</param>
    /// <param name="priority">The priority of the dispatch</param>
    /// <param name="token"></param>
    /// <typeparam name="TResult">The return value for the function</typeparam>
    /// <returns>A task that can be awaited, which is completed once the function returns on the dispatcher thread</returns>
    Task<T> InvokeAsync<T>(Func<T> function, DispatchPriority priority = DispatchPriority.Normal, CancellationToken token = default);

    /// <summary>
    /// Invokes the action asynchronously on this dispatcher thread. This differs from <see cref="InvokeAsync"/> where
    /// this method will cause any exception the actions throws and will throw it on the main thread, causing the app to crash,
    /// as apposed to silencing it and stuffing it into the Task object
    /// </summary>
    /// <param name="action"></param>
    /// <param name="priority"></param>
    void Post(Action action, DispatchPriority priority = DispatchPriority.Default);

    /// <summary>
    /// Process all queued events at and above the given priority. Once the task is complete,
    /// there will (most likely) be no more operations scheduled at and above the priority.
    /// <para>
    /// This is implemented by dispatching an empty action into <see cref="InvokeAsync"/> with
    /// the given priority. A non-async version of this method does not necessarily make sense
    /// </para>
    /// </summary>
    /// <param name="priority">The priority to process at and above</param>
    /// <returns>A task</returns>
    Task Process(DispatchPriority priority);

    /// <summary>
    /// Notifies the application to shutdown
    /// </summary>
    void InvokeShutdown();

    /// <summary>
    /// Creates a dispatcher timer
    /// </summary>
    IDispatcherTimer CreateTimer(DispatchPriority priority);

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
    void AwaitForCompletion(Task task) {
        if (!task.IsCompleted) {
            using CancellationTokenSource cts = new CancellationTokenSource();
            task.ContinueWith((t, theCts) => ((CancellationTokenSource) theCts!).Cancel(), cts, TaskContinuationOptions.ExecuteSynchronously);
            this.PushFrame(cts.Token);
        }

        if (!task.IsCanceled && task.Exception is AggregateException e) {
            ExceptionDispatchInfo.Throw(e.InnerExceptions.Count == 1 ? e.InnerExceptions[0] : e);
        }
    }
}