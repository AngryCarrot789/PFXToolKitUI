// 
// Copyright (c) 2025-2025 REghZy
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

using PFXToolKitUI.Interactivity.Windowing;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Interactivity.Dialogs;

/// <summary>
/// An interface that wraps a windowing mechanism to simplify dialog operations
/// </summary>
public interface IDialogOperation<T> {
    /// <summary>
    /// Gets the top level object that this dialog operation wraps
    /// </summary>
    ITopLevel TopLevel { get; }

    /// <summary>
    /// Gets the result of the operation. Returns <see cref="Optional{T}.Empty"/> if not completed or cancelled
    /// </summary>
    Optional<T> Result { get; }

    /// <summary>
    /// Gets whether the operation is completed. Note, the actual underlying window might
    /// not be closed or even closing yet, but will likely be so in the near future.
    /// </summary>
    bool IsCompleted { get; }
    
    /// <summary>
    /// Sets the result of this operation, making <see cref="Result"/> become valid and <see cref="IsCompleted"/> becomes true.
    /// This will also (likely) result in the dialog or overlay or underlying dialog content becoming closed/hidden
    /// </summary>
    /// <param name="result">The operation result</param>
    /// <exception cref="InvalidOperationException">Result is already set</exception>
    void SetResult(T result);

    /// <summary>
    /// Sets the operation as cancelled, meaning <see cref="Result"/> will be <see cref="Optional{T}.Empty"/> and <see cref="IsCompleted"/> becomes true.
    /// This will also (likely) result in the dialog or overlay or underlying dialog content becoming closed/hidden
    /// </summary>
    /// <exception cref="InvalidOperationException">Result is already set</exception>
    void SetCancelled();
    
    /// <summary>
    /// Returns a task that becomes completed when <see cref="IsCompleted"/> is true.
    /// If <see cref="SetResult"/> is called, then the task contains the value passed to that method.
    /// However, if <see cref="SetCancelled"/> is called or the underlying mechanism causes the dialog
    /// to close without a result, this task will be cancelled.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token to signal to stop waiting for the dialog result, causing the returned task to become cancelled immediately
    /// </param>
    /// <returns>A task that will either be completed with the result or cancelled</returns>
    Task<T> WaitForResultAsync(CancellationToken cancellationToken = default);
}