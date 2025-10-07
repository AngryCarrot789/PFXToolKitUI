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
    private readonly CancellationToken token;
    private readonly CancellationTokenRegistration registration;

    public CancellableTaskCompletionSource(CancellationToken token) : base(TaskCreationOptions.RunContinuationsAsynchronously) {
        this.token = token;
        if (token.CanBeCanceled) {
            this.registration = token.Register(this.SetCanceledCore);
            // this.Task.ContinueWith(
            //     static (t, tcs) => {
            //         ((CancellableTaskCompletionSource) tcs!).registration.Dispose();
            //     }, this, TaskContinuationOptions.ExecuteSynchronously
            // );
        }
    }

    private void SetCanceledCore() {
        this.TrySetCanceled(this.token);
    }

    /// <summary>
    /// Disposes the cancellation registration of the <see cref="CancellationToken"/>. 
    /// </summary>
    public void Dispose() {
        Debug.Assert(this.Task.IsCompleted, "Expected task to be completed at this point");
        this.registration.Dispose();
    }
}