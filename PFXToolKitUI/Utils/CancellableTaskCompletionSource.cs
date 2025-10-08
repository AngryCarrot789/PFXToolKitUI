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

// A helper class deriving TCS that allows an external cancellation signal to mark it as completed
public sealed class CancellableTaskCompletionSource : TaskCompletionSource, IDisposable {
    private static readonly Action<object?> s_HandleOnTokenCancelled = static t => ((CancellableTaskCompletionSource) t!).OnTokenCancelled();
    
    private readonly CancellationTokenRegistration registration;
    private readonly bool setSuccessfulInsteadOfCancelled;

    /// <summary>
    /// Creates an instance of <see cref="CancellableTaskCompletionSource"/>
    /// </summary>
    /// <param name="token">The token that can make this TCS become cancelled, or completed depending on the next parameter</param>
    /// <param name="setSuccessfulInsteadOfCancelled">
    /// Specifies if the cancellation of the token will mark this TCS as successful instead of cancelled.
    /// Will not change the behaviour of external calls to <see cref="TaskCompletionSource.SetCanceled()"/>
    /// or <see cref="TaskCompletionSource.TrySetCanceled()"/>
    /// </param>
    public CancellableTaskCompletionSource(CancellationToken token, bool setSuccessfulInsteadOfCancelled = false) : base(TaskCreationOptions.RunContinuationsAsynchronously) {
        this.setSuccessfulInsteadOfCancelled = setSuccessfulInsteadOfCancelled;
        if (token.CanBeCanceled) {
            if (token.IsCancellationRequested) {
                if (setSuccessfulInsteadOfCancelled) {
                    this.TrySetResult();
                }
                else {
                    this.TrySetCanceled(token);
                }
            }
            else {
                // If the token gets cancelled right as we're about to call Register, it
                // will still call OnTokenCancelled so there's no risk of deadlocked... supposedly 
                this.registration = token.Register(s_HandleOnTokenCancelled, this);
            }
        }
    }

    private void OnTokenCancelled() {
        if (this.setSuccessfulInsteadOfCancelled) {
            this.TrySetResult();
        }
        else {
            this.TrySetCanceled(this.registration.Token);
        }
        
        this.registration.Dispose();
    }

    /// <summary>
    /// Disposes the cancellation registration of the <see cref="CancellationToken"/>. 
    /// </summary>
    public void Dispose() {
        Debug.Assert(this.Task.IsCompleted, "Expected task to be completed at this point");
        this.registration.Dispose();
    }
}