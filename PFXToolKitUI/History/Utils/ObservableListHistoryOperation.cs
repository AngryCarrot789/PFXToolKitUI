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

using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.History.Utils;

/// <summary>
/// A class that allows for creating a builder object that captures the before-state and the after
/// state (at disposal of the builder) and allows for restoring both states by undoing and redoing.
/// </summary>
public static class ObservableListHistoryOperation {
    /// <summary>
    /// Creates a builder that, when disposed, adds a new history operation that allows for undoing
    /// and redoing any modifications to the source list during the life of the builder
    /// </summary>
    /// <param name="manager">The history manager that the operation should be added to</param>
    /// <param name="text">Used as <see cref="HistoryOperation.Text"/></param>
    /// <param name="sourceList">The list to use for restoring the states of</param>
    /// <typeparam name="T">The type of items in the list</typeparam>
    /// <returns>A builder that should be disposed once modifications are all recorded</returns>
    public static IObservableListHistoryBuilder CreateBuilder<T>(HistoryManager manager, string text, ObservableList<T> sourceList) {
        return new Builder<T>(manager, sourceList, text);
    }

    /// <summary>
    /// Creates a history operation for a single move operation. This assumes the item is currently at newIndex
    /// </summary>
    /// <param name="sourceList">The list to use for restoring the states of</param>
    /// <param name="oldIndex">The item's previous index</param>
    /// <param name="newIndex">The item's new index</param>
    /// <typeparam name="T">The type of items in the list</typeparam>
    /// <returns>A history operation that can undo and redo the move operation</returns>
    public static HistoryOperation ForMove<T>(string text, ObservableList<T> sourceList, int oldIndex, int newIndex) {
        return new MoveItemInListHistoryOperation<T>(text, sourceList, oldIndex, newIndex);
    }

    private class MoveItemInListHistoryOperation<T> : HistoryOperation {
        private readonly ObservableList<T> myList;
        private readonly int oldIdx, newIdx;
        private readonly T theItem;
        
        public override string Text { get; }

        public MoveItemInListHistoryOperation(string text, ObservableList<T> sourceList, int oldIndex, int newIndex) {
            this.Text = text;
            this.myList = sourceList;
            this.oldIdx = oldIndex;
            this.newIdx = newIndex;
            this.theItem = sourceList[newIndex];
        }

        protected override Task OnUndo() {
            if (this.oldIdx >= this.myList.Count || this.newIdx >= this.myList.Count)
                throw new InvalidHistoryException("Unexpected list state");
            if (!typeof(T).IsValueType)
                InvalidHistoryException.Assert((object) this.theItem! == (object) this.myList[this.newIdx]!, "Unexpected list state; items in wrong arrangement");

            this.myList.Move(this.newIdx, this.oldIdx);
            return Task.CompletedTask;
        }

        protected override Task OnRedo() {
            if (this.oldIdx >= this.myList.Count || this.newIdx >= this.myList.Count)
                throw new InvalidHistoryException("Unexpected list state");
            if (!typeof(T).IsValueType)
                InvalidHistoryException.Assert((object) this.theItem! == (object) this.myList[this.oldIdx]!, "Unexpected list state; items in wrong arrangement");

            this.myList.Move(this.oldIdx, this.newIdx);
            return Task.CompletedTask;
        }
    }

    private class Builder<T> : IObservableListHistoryBuilder {
        private readonly List<T> myBeforeState;
        private readonly HistoryManager manager;
        private readonly ObservableList<T> target;
        private readonly string myText;

        public event Action? BeforeUndo, BeforeRedo, AfterUndo, AfterRedo;

        public Builder(HistoryManager manager, ObservableList<T> target, string text) {
            this.manager = manager;
            this.target = target;
            this.myText = text;
            this.myBeforeState = this.target.ToList();
        }

        public void Dispose() {
            if (this.myBeforeState == null)
                throw new InvalidOperationException("Before-state not captured");
            this.manager.AddOperation(new ListTransactionOperationImpl(this));
        }

        private sealed class ListTransactionOperationImpl : HistoryOperation {
            private readonly ObservableList<T> target;
            private readonly List<T> myBeforeState;
            private readonly List<T> myAfterState;
            private readonly Action? beforeUndo, beforeRedo, afterUndo, afterRedo;

            public override string Text { get; }
            
            public ListTransactionOperationImpl(Builder<T> builder) {
                this.target = builder.target;
                this.myBeforeState = builder.myBeforeState;
                this.myAfterState = this.target.ToList();
                this.beforeUndo = builder.BeforeUndo;
                this.beforeRedo = builder.BeforeRedo;
                this.afterUndo = builder.AfterUndo;
                this.afterRedo = builder.AfterRedo;
                this.Text = builder.myText;
            }

            protected override Task OnUndo() {
                InvalidHistoryException.Assert(this.target.SequenceEqual(this.myAfterState), "Difference sequences");

                this.beforeUndo?.Invoke();
                this.target.Clear();
                this.target.AddRange(this.myBeforeState);
                this.afterUndo?.Invoke();
                return Task.CompletedTask;
            }

            protected override Task OnRedo() {
                InvalidHistoryException.Assert(this.target.SequenceEqual(this.myBeforeState), "Difference sequences");

                this.beforeRedo?.Invoke();
                this.target.Clear();
                this.target.AddRange(this.myAfterState);
                this.afterRedo?.Invoke();
                return Task.CompletedTask;
            }
        }
    }
}

public interface IObservableListHistoryBuilder : IDisposable {
    event Action BeforeUndo, BeforeRedo;
    event Action AfterUndo, AfterRedo;
}