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
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.RDA;

namespace PFXToolKitUI.Activities;

public delegate void TotalCompletionChangedEventHandler(CompletionState state);

/// <summary>
/// Represents the state of a completable action
/// </summary>
public abstract class CompletionState {
    private readonly Stack<CompletionRange> ranges;
    private double totalMultiplier;

    /// <summary>
    /// Gets or sets a value between 0.0 and 1.0 that represents how far completed the entire operation has completed
    /// </summary>
    public abstract double TotalCompletion { get; set; }

    public event TotalCompletionChangedEventHandler? CompletionValueChanged;

    protected CompletionState() {
        this.ranges = new Stack<CompletionRange>();
        this.totalMultiplier = 1.0;
    }

    /// <summary>
    /// Raises the <see cref="CompletionValueChanged"/> event
    /// </summary>
    protected virtual void OnCompletionValueChanged() {
        this.CompletionValueChanged?.Invoke(this);
    }

    /// <summary>
    /// Pushes a new completion range. The difference between min and max is the completion
    /// value that will be added to the parent range's completion value.
    /// <para>
    /// A completion range should be pushed when you're about to begin an 'operation phase', that is,
    /// something that can have a completion percentage. That operation itself can push its own completion
    /// ranges, but it is your job to push a range which represents how much actual work that operation
    /// does relative to the current operation
    /// </para>
    /// <para>
    /// The reason for a min and max is mainly for clarity so that you can identify possible mis-uses and bugs.
    /// In a code block, the total amount of range pushed should equal 1.0. For example, you
    /// push 0.0->0.2, 0.2->0.7, 0.7->1.0, where the differences between max and min for those
    /// cases sum to 1.0. If they don't, then it probably means this method was used incorrectly
    /// </para>
    /// <para>
    /// An alternative usage is specifying min as 0 and max as <c>modulus / count</c>. This allows
    /// progressing by <c>1.0</c> during the loop when <c>i</c> is divisible by <c>modulus</c> to
    /// ensure the completion is 100% by the end of the loop
    /// <code>
    /// <![CDATA[
    /// using var _ = activity.CompletionState.PushCompletionRange(0.0, modulus / count);
    /// for (int i = 0; i < count; i++) {
    ///     if (i % modulus == 0) {
    ///         activity.CompletionState.OnProgress(1);
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </para>
    /// <para>
    /// And another alternative is:
    /// <code>
    /// <![CDATA[
    /// using var token = activity.CompletionState.PushCompletionRange(0, 1.0 / count);
    /// for (int i = 0; i < count; i += incr) {
    ///     activity.CompletionState.OnProgress(incr);
    /// }
    /// ]]>
    /// </code>
    /// </para>
    /// </summary>
    /// <param name="min">The minimum bound</param>
    /// <param name="max">The maximum bound</param>
    /// <param name="fillRemainingOnCompleted">Passed to <see cref="PopCompletionRange"/> when the token is disposed</param>
    /// <returns>
    /// A disposable struct that, when <see cref="PopCompletionStateRangeToken.Dispose"/> is called, calls <see cref="PopCompletionRange"/>.
    /// This struct can be used in a using statement, where the 'operation' is inside the using block, for convenience
    /// and clean code sakes. The struct does not need to be used; <see cref="PopCompletionRange"/> can be called manually
    /// </returns>
    public PopCompletionStateRangeToken PushCompletionRange(double min, double max, bool fillRemainingOnCompleted = true) {
        CompletionRange range = new CompletionRange(max - min, this.totalMultiplier, this.TotalCompletion);
        this.totalMultiplier *= range.range;
        this.ranges.Push(range);
        return new PopCompletionStateRangeToken(this, fillRemainingOnCompleted);
    }

    /// <summary>
    /// Pops the completion range on the top of the stack
    /// </summary>
    /// <param name="fillRemainingOnCompleted">
    /// Sets the current progress to 1.0 for the current stack before popping the top range.
    /// This is to ensure the progression is complete is someone forgets to update the progress
    /// </param>
    public void PopCompletionRange(bool fillRemainingOnCompleted = true) {
        if (this.ranges.Count < 1)
            throw new InvalidOperationException("Too many completion ranges popped: the stack is empty!");

        // Just set the progress to 100%
        if (fillRemainingOnCompleted)
            this.TotalCompletion = this.ranges.Peek().prevTotalCompletion + this.totalMultiplier;

        CompletionRange popped = this.ranges.Pop();
        this.totalMultiplier = popped.prevMultiplier;
    }

    /// <summary>
    /// Adds the given value to <see cref="TotalCompletion"/>. The final added amount depends on the
    /// completion ranges currently on the stack. If there are none, then this method is the same as
    /// adding the value to <see cref="TotalCompletion"/> directly. During a range push, the total progress should add up to 1.0
    /// </summary>
    /// <param name="value">The value to append (multiplied based on the current ranges on the stack)</param>
    public void OnProgress(double value) {
        if (this.ranges.Count > 0) {
            this.TotalCompletion += this.totalMultiplier * value;
        }
        else {
            // assert totalMultiplier == 1.0
            this.TotalCompletion += value;
        }
    }

    /// <summary>
    /// Sets the given value as the total completion. The value <see cref="TotalCompletion"/>
    /// becomes depends on the ranges on the stack. If there are none, then
    /// <see cref="TotalCompletion"/> is set directly
    /// </summary>
    /// <param name="value">The value to append (multiplied based on the current ranges on the stack)</param>
    public void SetProgress(double value) {
        if (this.ranges.TryPeek(out CompletionRange top)) {
            this.TotalCompletion = top.prevTotalCompletion + (this.totalMultiplier * value);
        }
        else {
            // assert totalMultiplier == 1.0
            this.TotalCompletion = value;
        }
    }

    /// <summary>
    /// Saves the current state of pushed completion ranges and total completion
    /// that may be restored when the returned value is disposed
    /// </summary>
    /// <returns>
    /// A value that when disposed will restore the pushed completion ranges and total completion
    /// </returns>
    public State SaveState() => new State(this);

    public readonly struct State(CompletionState state) : IDisposable {
        private readonly CompletionRange[] ranges = state.ranges.ToArray();
        private readonly double totalMultiplier = state.totalMultiplier;
        private readonly double totalCompletion = state.TotalCompletion;

        public void Dispose() {
            state.ranges.Clear();
            foreach (CompletionRange range in this.ranges)
                state.ranges.Push(range);
            
            state.totalMultiplier = this.totalMultiplier;
            state.TotalCompletion = this.totalCompletion;
        }
    }

    private readonly struct CompletionRange(double range, double prevMultiplier, double prevTotalCompletion) {
        public readonly double range = range;
        public readonly double prevMultiplier = prevMultiplier;
        public readonly double prevTotalCompletion = prevTotalCompletion;

        public override string ToString() {
            return $"Range={this.range:F4} ~ PrevMultiplier={this.prevMultiplier:F4} ~ PrevTotalCompletion={this.prevTotalCompletion:F4}";
        }
    }
    
    public static void TestCompletionRangeFunctionality() {
        SimpleCompletionState tracker = new SimpleCompletionState();
        using (tracker.PushCompletionRange(0.0, 0.5)) {
            // Begin: CloseActive
            // parent range = 0.5, so 0.5 * 0.25 = 0.125.
            // TotalCompletion = 0.0 + 0.125
            tracker.OnProgress(0.25);
            // parent range = 0.5, so 0.5 * 0.75 = 0.375
            // TotalCompletion = 0.125 + 0.375 = 0.5
            tracker.OnProgress(0.75);
            // assert tracker.TotalCompletion == 0.5
            // End: CloseActive
        }

        using (tracker.PushCompletionRange(0.5, 1.0)) {
            // Begin: OpenProject

            using (tracker.PushCompletionRange(0.0, 0.25)) {
                // Begin: PreLoad

                using (tracker.PushCompletionRange(0.0, 0.1)) {
                    // Begin: ProcessPreLoad
                    tracker.SetProgress(0.7);
                    tracker.SetProgress(0.8);
                    tracker.SetProgress(0.2);
                    tracker.SetProgress(0.5);
                    tracker.OnProgress(0.5);
                    // End: ProcessPreLoad
                }

                tracker.OnProgress(0.4);
                tracker.OnProgress(0.5);
                // End: PreLoad
            }

            using (tracker.PushCompletionRange(0.25, 0.5)) {
                // Begin: PostLoad
                tracker.OnProgress(0.2);
                tracker.OnProgress(0.8);
                // End: PostLoad
            }

            using (tracker.PushCompletionRange(0.5, 1.0)) {
                // Begin: PostLoad
                tracker.OnProgress(0.3);
                tracker.OnProgress(0.6);
                tracker.OnProgress(0.1);
                // End: PostLoad
            }

            // End: OpenProject
        }

        if (!DoubleUtils.AreClose(tracker.TotalCompletion, 1.0)) {
            Debugger.Break(); // test failed
            throw new Exception("Test failed. Completion ranges do not function as expected");
        }
    }
}

public sealed class EmptyCompletionState : CompletionState {
    public override double TotalCompletion {
        get => 0.0;
        set { }
    }
}

/// <summary>
/// A completion state that supports changing the completion value from any thread,
/// and raises the <see cref="CompletionState.CompletionValueChanged"/> event only on the main thread
/// </summary>
public sealed class ConcurrentCompletionState : CompletionState {
    private readonly RapidDispatchActionEx updateCompletionValue;
    private double myCompletion;

    public override double TotalCompletion {
        get => this.myCompletion;
        set {
            double previous = Interlocked.Exchange(ref this.myCompletion, value);
            if (!DoubleUtils.AreClose(previous, value))
                this.updateCompletionValue?.InvokeAsync();
        }
    }

    public ConcurrentCompletionState(DispatchPriority priority) {
        this.updateCompletionValue = RapidDispatchActionEx.ForSync(this.OnCompletionValueChanged, priority);
    }
}

/// <summary>
/// A simple implementation of <see cref="CompletionState"/>
/// </summary>
public sealed class SimpleCompletionState : CompletionState {
    private double myCompletion;

    public override double TotalCompletion {
        get => this.myCompletion;
        set {
            if (DoubleUtils.AreClose(this.myCompletion, value))
                return;

            this.myCompletion = value;
            base.OnCompletionValueChanged();
        }
    }

    public new void OnCompletionValueChanged() {
        base.OnCompletionValueChanged();
    }
}

/// <summary>
/// A struct used to automatically pop a completion range from a tracker, to make the code easier to
/// #read. This can only pop once, then calling Dispose again does nothing
/// </summary>
public struct PopCompletionStateRangeToken : IDisposable {
    private CompletionState? state;
    private readonly bool fillRemainingOnCompleted;

    public PopCompletionStateRangeToken(CompletionState state, bool fillRemainingOnCompleted) {
        this.state = state;
        this.fillRemainingOnCompleted = fillRemainingOnCompleted;
    }

    public void Dispose() {
        CompletionState? t = this.state;
        this.state = null;
        t?.PopCompletionRange(this.fillRemainingOnCompleted);
    }
}