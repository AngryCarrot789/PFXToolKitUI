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

namespace PFXToolKitUI.Utils;

/// <summary>
/// A class that stores a mutable disposable value that can be disposed and re-generated,
/// and manages its disposal when used by different thread.
/// </summary>
/// <typeparam name="T">The type of value</typeparam>
public class DisposableRef<T> where T : IDisposable {
    private enum DisposedState {
        Valid,
        Queued,
        Disposed
    }

    private int usageCount;
    private DisposedState disposeState; // 0 == valid, 1 == dispose queued, 2 == disposed
    private event EventHandler? UsageEmpty;

    public T Value { get; }

    public bool IsWaitingDisposal => this.disposeState == DisposedState.Queued;

    /// <summary>
    /// The constructor for a disposable reference
    /// </summary>
    /// <param name="value">The value that gets stored</param>
    /// <param name="isInitiallyDisposed">True to mark the value as disposed to implement lazily loading of the value</param>
    public DisposableRef(T value, bool isInitiallyDisposed = false) {
        this.Value = value;
        if (isInitiallyDisposed)
            this.disposeState = DisposedState.Disposed;
    }

    /// <summary>
    /// Tries to begin using this reference. If the value is disposed, then this method returns false. If the caller
    /// has ownership over the resource, then it it should be initialised after this call and then <see cref="ResetAndBeginUsage"/>
    /// should be called
    /// <para>
    /// This method MUST be called while a lock to this object's instance is acquired
    /// </para>
    /// </summary>
    /// <returns>True if not disposed, otherwise false</returns>
    public bool TryBeginUsage() {
        if (this.disposeState == DisposedState.Disposed) {
            return false;
        }

        this.disposeState = DisposedState.Valid;
        this.usageCount++;
        return true;
    }

    /// <summary>
    /// Forces rendering to begin, assuming <see cref="TryBeginUsage"/> previously returned false and the value is now initialised
    /// <para>
    /// This method MUST be called while a lock to this object's instance is acquired
    /// </para>
    /// </summary>
    public void ResetAndBeginUsage() {
        this.disposeState = DisposedState.Valid;
        this.usageCount++;
    }

    /// <summary>
    /// Tries to begin using this resource as an owner. If the value is disposed, then the resetter action is called
    /// and usage begins
    /// <para>
    /// This method automatically acquires the lock on this instance
    /// </para>
    /// </summary>
    /// <param name="owner">Passed to the resetter</param>
    /// <param name="resetter">The resetter to reset the value (to un-dispose it)</param>
    /// <typeparam name="TOwner">The owner type</typeparam>
    public void BeginUsage<TOwner>(TOwner owner, Action<TOwner, T> resetter) {
        lock (this) {
            if (this.disposeState == DisposedState.Disposed) {
                this.disposeState = DisposedState.Valid;
                resetter(owner, this.Value);
            }

            this.usageCount++;
        }
    }

    /// <summary>
    /// Completes a usage phase of this reference.
    /// <para>
    /// This method automatically acquires the lock on this instance
    /// </para>
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Excessive calls to CompleteUsage, or the lock was not acquired causing this object to become corrupted
    /// </exception>
    public void CompleteUsage() {
        lock (this) {
            if (this.usageCount < 1)
                throw new InvalidOperationException("Expected a usage beforehand. Possible bug, excessive calls to CompleteUsage?");
            if (--this.usageCount == 0) {
                if (this.disposeState == DisposedState.Queued)
                    this.DisposeInternal();
                this.UsageEmpty?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Marks the value to be disposed if in use, or disposes of the resource right now if not in use.
    /// <para>
    /// This method automatically acquires the lock on this instance
    /// </para>
    /// </summary>
    public void Dispose() {
        lock (this) {
            if (this.usageCount > 0) {
                this.disposeState = DisposedState.Queued;
            }
            else {
                this.DisposeInternal();
            }
        }
    }

    private void DisposeInternal() {
        this.disposeState = DisposedState.Disposed;
        this.Value!.Dispose();
    }

    public async Task WaitForNoUsages() {
        if (this.usageCount < 1)
            return;

        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        EventHandler handler = null;
        handler = (sender, args) => {
            tcs.SetResult(true);
            this.UsageEmpty -= handler;
        };

        this.UsageEmpty += handler;
        await tcs.Task;
    }
}