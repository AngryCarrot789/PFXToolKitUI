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
using System.Runtime.ExceptionServices;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.History;

public delegate void HistoryManagerEventHandler(HistoryManager sender);

public delegate void HistoryManagerOperationEventHandler(HistoryManager sender, HistoryOperation operation);

/// <summary>
/// A class that manages a collection of undo-able and redo-able actions
/// </summary>
public sealed class HistoryManager {
    public static readonly DataKey<HistoryManager> DataKey = DataKeys.Create<HistoryManager>(nameof(HistoryManager));
    private static readonly ThreadLocal<HistoryManager?> threadActiveHistory = new ThreadLocal<HistoryManager?>();

    private readonly LinkedList<HistoryOperation> undoList;
    private readonly LinkedList<HistoryOperation> redoList;
    private readonly Stack<List<HistoryOperation>> mergingStack = new Stack<List<HistoryOperation>>();

    private bool isUndoInProgress;
    private bool isRedoInProgress;

    /// <summary>
    /// Returns true when another undo operation is still running
    /// </summary>
    public bool IsUndoInProgress {
        get => this.isUndoInProgress;
        private set => PropertyHelper.SetAndRaiseINE(ref this.isUndoInProgress, value, this, static t => t.IsUndoInProgressChanged?.Invoke(t));
    }

    /// <summary>
    /// Returns true when another redo operation is still running
    /// </summary>
    public bool IsRedoInProgress {
        get => this.isRedoInProgress;
        private set => PropertyHelper.SetAndRaiseINE(ref this.isRedoInProgress, value, this, static t => t.IsRedoInProgressChanged?.Invoke(t));
    }

    public bool IsBusy => this.IsUndoInProgress || this.IsRedoInProgress;

    /// <summary>
    /// Returns the number of undo-able actions. This does not count actions in the current execution section
    /// </summary>
    public bool HasUndoOperations => this.undoList.Count > 0;

    /// <summary>
    /// Returns the number of redo-able actions. This does not count actions in the current execution section
    /// </summary>
    public bool HasRedoOperations => this.redoList.Count > 0;

    public event HistoryManagerEventHandler? IsUndoInProgressChanged;
    public event HistoryManagerEventHandler? IsRedoInProgressChanged;
    public event HistoryManagerOperationEventHandler? BeforeUndo, BeforeRedo;
    public event HistoryManagerOperationEventHandler? AfterUndo, AfterRedo;

    private int globalStackDepth;

    public HistoryManager() {
        this.undoList = new LinkedList<HistoryOperation>();
        this.redoList = new LinkedList<HistoryOperation>();
    }

    // public void LinkToParent(HistoryManager newParent) {
    //     this.CheckNotPerformingUndoOrRedo();
    //     this.parent = newParent;
    //     if (newParent != null) {
    //         this.ClearBuffers();
    //     }
    // }

    private void ClearBuffers() {
        this.ClearRedoBuffer();
        this.ClearUndoBuffer();
    }

    private void ClearUndoBuffer() {
        foreach (HistoryOperation t in this.undoList)
            t.Dispose();

        this.undoList.Clear();
    }

    private void ClearRedoBuffer() {
        foreach (HistoryOperation t in this.redoList)
            t.Dispose();

        this.redoList.Clear();
    }

    private void RemoveFirstUndoable() {
        this.undoList.First!.Value.Dispose();
        this.undoList.RemoveFirst();
    }

    /// <summary>
    /// Clears all undo-able and redo-able actions
    /// </summary>
    public void Clear() {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();

        this.CheckNotPerformingUndoOrRedo();
        this.ClearBuffers();
    }

    public void AddOperation(string text, Func<Task> undo, Func<Task> redo, Action? dispose = null) {
        this.AddOperation(new AnonymousAsyncHistoryOperation(text, undo, redo, dispose));
    }

    public void AddOperation(string text, Action undo, Action redo, Action? dispose = null) {
        this.AddOperation(new AnonymousHistoryOperation(text, undo, redo, dispose));
    }

    /// <summary>
    /// Applies the operation and adds it to the undo stack or to a joined history operation if 
    /// </summary>
    /// <param name="operation"></param>
    public void AddOperation(HistoryOperation operation) {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();
        this.CheckNotPerformingUndoOrRedo();

        if (this.mergingStack.Count > 0) {
            this.mergingStack.Peek().Add(operation);
        }
        else {
            this.ClearRedoBuffer();
            this.undoList.AddLast(operation);
            while (this.undoList.Count > 500) {
                this.RemoveFirstUndoable();
            }
        }
    }

    public Task UndoAsync() => this.PerformUndoOrRedo(true);

    public Task RedoAsync() => this.PerformUndoOrRedo(false);

    private async Task PerformUndoOrRedo(bool isUndo) {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();
        this.CheckStateForUndoOrRedo(isUndo);

        LinkedList<HistoryOperation> srcList = isUndo ? this.undoList : this.redoList;
        LinkedList<HistoryOperation> dstList = isUndo ? this.redoList : this.undoList;

        HistoryOperation operation = srcList.Last!.Value;

        try {
            if (isUndo) {
                this.BeforeUndo?.Invoke(this, operation);
                this.IsUndoInProgress = true;
                await operation.Undo();
                this.AfterUndo?.Invoke(this, operation);
            }
            else {
                this.BeforeRedo?.Invoke(this, operation);
                this.IsRedoInProgress = true;
                await operation.Redo();
                this.AfterRedo?.Invoke(this, operation);
            }
        }
        catch (InvalidHistoryException) {
            this.ClearBuffers();
            throw;
        }
        catch (Exception e) {
            this.ClearBuffers();
            throw new InvalidHistoryException("Unexpected error while performing " + (isUndo ? "undo" : "redo"), e);
        }
        finally {
            if (isUndo) {
                this.IsUndoInProgress = false;
            }
            else {
                this.IsRedoInProgress = false;
            }
        }

        srcList.RemoveLast();
        dstList.AddLast(operation);
    }

    private void CheckNotPerformingUndoOrRedo() {
        if (this.IsUndoInProgress)
            throw new InvalidOperationException("Undo is in progress");
        if (this.IsRedoInProgress)
            throw new InvalidOperationException("Redo is in progress");
    }

    private void CheckStateForUndoOrRedo(bool isUndo) {
        this.CheckNotPerformingUndoOrRedo();
        if (isUndo) {
            if (this.undoList.Count < 1)
                throw new InvalidOperationException("Nothing to undo");
        }
        else if (this.redoList.Count < 1)
            throw new InvalidOperationException("Nothing to redo");
    }

    public static IDisposable Push(HistoryManager historyManager) {
        int count = historyManager.globalStackDepth;
        if (count == 0) {
            Debug.Assert(threadActiveHistory.Value == null);
            threadActiveHistory.Value = historyManager;
        }

        historyManager.globalStackDepth = count + 1;
        return new ActiveHistory(historyManager);
    }

    private static void Pop(HistoryManager historyManager) {
        int count = historyManager.globalStackDepth;
        Debug.Assert(count > 0);
        if (count == 1) {
            Debug.Assert(threadActiveHistory.Value == historyManager);
            threadActiveHistory.Value = null;
        }

        historyManager.globalStackDepth = count - 1;
    }

    private sealed class ActiveHistory(HistoryManager historyManager) : IDisposable {
        private readonly Thread originalThread = Thread.CurrentThread;
        private HistoryManager? manager = historyManager;

        public void Dispose() {
            ObjectDisposedException.ThrowIf(this.manager == null, typeof(ActiveHistory));
            if (Thread.CurrentThread != this.originalThread)
                throw new InvalidOperationException("Wrong thread");
            Pop(this.manager);
            this.manager = null;
        }
    }

    /// <summary>
    /// Enters a merging state, where all history operations added to the current merging stack are grouped as a single operation.
    /// </summary>
    /// <returns>An object that, when disposed, will end the merging section</returns>
    public IDisposable BeginMergedSection() {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();

        this.OnBeginExecutionSection();
        return new MergedSection(this);
    }

    private void OnBeginExecutionSection() {
        this.mergingStack.Push(new List<HistoryOperation>());
    }

    private void OnEndExecutionSection() {
        List<HistoryOperation> list = this.mergingStack.Pop();
        if (list.Count > 0) {
            this.AddOperation(new JoinedHistoryOperation(list));
        }
    }

    private class MergedSection(HistoryManager manager) : IDisposable {
        private HistoryManager? manager = manager;

        public void Dispose() {
            ApplicationPFX.Instance.Dispatcher.VerifyAccess();

            if (this.manager != null) {
                this.manager.OnEndExecutionSection();
                this.manager = null;
            }
        }
    }

    private sealed class JoinedHistoryOperation(List<HistoryOperation> operations) : HistoryOperation {
        private string? myText;
        private bool isDisposed;

        public override string Text => this.myText ??= string.Join(", ", operations.Select(x => x.Text));

        protected override async Task OnUndo() {
            if (this.isDisposed)
                throw new ObjectDisposedException("this", "Operation disposed");

            for (int i = operations.Count - 1; i >= 0; i--) {
                HistoryOperation operation = operations[i];
                await operation.Undo();
            }
        }

        protected override async Task OnRedo() {
            if (this.isDisposed)
                throw new ObjectDisposedException("this", "Operation disposed");

            foreach (HistoryOperation operation in operations) {
                await operation.Redo();
            }
        }

        protected override void OnDispose() {
            if (!this.isDisposed) {
                this.isDisposed = true;
                foreach (HistoryOperation operation in operations) {
                    operation.Dispose();
                }
            }
        }
    }

    private class AnonymousAsyncHistoryOperation(string text, Func<Task> undo, Func<Task> redo, Action? dispose) : HistoryOperation {
        public override string Text { get; } = text;

        protected override Task OnUndo() => undo();
        protected override Task OnRedo() => redo();
        protected override void OnDispose() => dispose?.Invoke();
    }

    private class AnonymousHistoryOperation(string text, Action undo, Action redo, Action? dispose) : HistoryOperation {
        public override string Text { get; } = text;

        protected override Task OnUndo() {
            try {
                undo();
                return Task.CompletedTask;
            }
            catch (Exception e) {
                return Task.FromException(e);
            }
        }

        protected override Task OnRedo() {
            try {
                redo();
                return Task.CompletedTask;
            }
            catch (Exception e) {
                return Task.FromException(e);
            }
        }

        protected override void OnDispose() => dispose?.Invoke();
    }
}