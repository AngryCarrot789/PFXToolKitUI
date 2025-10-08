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
using System.Runtime.CompilerServices;

namespace PFXToolKitUI;

/// <summary>
/// Provides a mechanism for invoking actions on a specific thread
/// </summary>
public interface IDispatcher {
    private static readonly Action EmptyAction = () => {
    };

    /// <summary>
    /// Determines whether the calling thread is the thread associated with this dispatcher
    /// </summary>
    bool CheckAccess();

    /// <summary>
    /// Throws an exception if the calling thread is not the thread associated with this dispatcher
    /// </summary>
    void VerifyAccess() {
        if (!this.CheckAccess()) {
            [DoesNotReturn]
            [MethodImpl(MethodImplOptions.NoInlining)]
            static void ThrowInvalidThread() => throw new InvalidOperationException("Call from invalid thread");

            ThrowInvalidThread();
        }
    }

    /// <summary>
    /// Asynchronously executes the function on the dispatcher thread
    /// </summary>
    /// <param name="action">The action to invoke</param>
    /// <param name="priority">The dispatch priority</param>
    /// <param name="captureContext">
    /// Specifies whether to capture the current execution context and restore it while executing the callback
    /// </param>
    /// <param name="token">A token to cancel the operation, before it runs</param>
    /// <returns>A task that becomes completed once the action completes</returns>
    /// <remarks>
    /// If the action throws an exception (including OCE), it will be caught and the task becomes faulted.
    /// The task only becomes cancelled when the operation has not started yet and the token becomes cancelled
    /// </remarks>
    Task InvokeAsync(Action action, DispatchPriority priority = DispatchPriority.Normal, bool captureContext = false, CancellationToken token = default);

    /// <summary>
    /// Asynchronously executes the function on the dispatcher thread and produces the function's return value as a result in the task
    /// </summary>
    /// <param name="function">The function to invoke</param>
    /// <param name="priority">The dispatch priority</param>
    /// <param name="captureContext">
    /// Specifies whether to capture the current execution context and restore it while executing the callback
    /// </param>
    /// <param name="token">A token to cancel the operation, before it runs</param>
    /// <typeparam name="T">The type of return value</typeparam>
    /// <returns>A task that becomes completed with the return value of the function</returns>
    /// <remarks>
    /// If the action throws an exception (including OCE), it will be caught and the task becomes faulted.
    /// The task only becomes cancelled when the operation has not started yet and the token becomes cancelled
    /// </remarks>
    Task<T> InvokeAsync<T>(Func<T> function, DispatchPriority priority = DispatchPriority.Normal, bool captureContext = false, CancellationToken token = default);

    /// <summary>
    /// Invokes the action asynchronously on this dispatcher thread. This differs from <see cref="InvokeAsync"/> where
    /// this method will cause any exception the actions throws and will throw it on the main thread, causing the app to crash,
    /// as apposed to silencing it and stuffing it into the Task object
    /// </summary>
    /// <param name="action">The callback to invoke</param>
    /// <param name="priority">The priority to invoke the callback at</param>
    /// <param name="captureContext">
    /// Specifies whether to capture the current execution context and restore it while executing the callback
    /// </param>
    void Post(Action action, DispatchPriority priority = DispatchPriority.Default, bool captureContext = false);

    /// <summary>
    /// Invokes the action asynchronously on this dispatcher thread. This differs from <see cref="InvokeAsync"/> where
    /// this method will cause any exception the actions throws and will throw it on the main thread, causing the app to crash,
    /// as apposed to silencing it and stuffing it into the Task object
    /// </summary>
    /// <param name="action">The callback to invoke</param>
    /// <param name="state">The state object to pass to the callback</param>
    /// <param name="priority">The priority to invoke the callback at</param>
    /// <param name="captureContext">
    /// Specifies whether to capture the current execution context and restore it while executing the callback
    /// </param>
    void Post(SendOrPostCallback action, object? state, DispatchPriority priority = DispatchPriority.Default, bool captureContext = false);

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
    Task Process(DispatchPriority priority) => this.InvokeAsync(EmptyAction, priority, false);

    /// <summary>
    /// Posts a shutdown request to the dispatcher
    /// </summary>
    void PostShutdown(DispatchPriority priority) => this.Post(static s => ((IDispatcher) s!).Shutdown(), this, priority);

    /// <summary>
    /// Shuts down the dispatcher, preventing any actions from being enqueued
    /// </summary>
    void Shutdown();

    /// <summary>
    /// Creates a dispatcher timer
    /// </summary>
    IDispatcherTimer CreateTimer(DispatchPriority priority);

    /// <summary>
    /// Tries to get the dispatcher frame manager, which supports run/message loops.
    /// This allows, for example, waiting for an asynchronous operation to complete without necessarily blocking the UI thread
    /// </summary>
    /// <param name="frameManager">The frame manager, or null, if not supported by this dispatcher</param>
    /// <returns>True if this dispatcher supports run loops</returns>
    /// <remarks>This feature is generally only supported on desktop, whereas mobile will not</remarks>
    bool TryGetFrameManager([NotNullWhen(true)] out IDispatcherFrameManager? frameManager);
}