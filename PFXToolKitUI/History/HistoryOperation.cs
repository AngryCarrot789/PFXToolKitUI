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

namespace PFXToolKitUI.History;

/// <summary>
/// The base class for an operation that supports undoing and redoing some action
/// </summary>
public abstract class HistoryOperation {
    private bool lastActionWasUndo, isDisposed;

    /// <summary>
    /// Gets a short readable description of what this operation does. For example, if this operation deletes items, this may be "Delete 4 items".
    /// <para>
    /// This is used to show a dialog if its undo or redo operation prolongs for long enough
    /// </para>
    /// </summary>
    public abstract string Text { get; }

    protected HistoryOperation() {
    }

    /// <summary>
    /// Reverse the operation.
    /// </summary>
    /// <exception cref="InvalidHistoryException">The application was not in the expected state</exception>
    public async Task Undo() {
        ObjectDisposedException.ThrowIf(this.isDisposed, this);
        if (this.lastActionWasUndo)
            throw new InvalidOperationException("Cannot undo without first redoing");

        this.lastActionWasUndo = true;
        await this.OnUndo();
    }

    /// <summary>
    /// Execute the operation.
    /// </summary>
    /// <exception cref="InvalidHistoryException">The application was not in the expected state</exception>
    public async Task Redo() {
        ObjectDisposedException.ThrowIf(this.isDisposed, this);
        if (!this.lastActionWasUndo)
            throw new InvalidOperationException("Cannot redo without first undoing");

        this.lastActionWasUndo = false;
        await this.OnRedo();
    }

    /// <summary>
    /// Disposes this history action. This is called when it is no longer reachable/deleted (e.g. it was undone then another action was executed, meaning the history got cleared)
    /// </summary>
    public void Dispose() {
        if (!this.isDisposed) {
            this.isDisposed = true;
            this.OnDispose();
        }
    }
    
    protected abstract Task OnUndo();
    
    protected abstract Task OnRedo();

    protected virtual void OnDispose() {
    }
}