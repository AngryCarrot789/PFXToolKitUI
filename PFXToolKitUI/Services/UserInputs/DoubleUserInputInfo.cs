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

using System.Collections.ObjectModel;
using PFXToolKitUI.Utils.Debouncing;
using PFXToolKitUI.Utils.Events;
using PFXToolKitUI.Utils.Reactive;

namespace PFXToolKitUI.Services.UserInputs;

public class DoubleUserInputInfo : BaseTextUserInputInfo {
    public static IEventObservable<DoubleUserInputInfo> TextAObservable => field ??= Observable.ForEvent<DoubleUserInputInfo>((s, e) => s.TextAChanged += e, (s, e) => s.TextAChanged -= e);
    public static IEventObservable<DoubleUserInputInfo> TextBObservable => field ??= Observable.ForEvent<DoubleUserInputInfo>((s, e) => s.TextBChanged += e, (s, e) => s.TextBChanged -= e);
    public static IEventObservable<DoubleUserInputInfo> LabelAObservable => field ??= Observable.ForEvent<DoubleUserInputInfo>((s, e) => s.LabelAChanged += e, (s, e) => s.LabelAChanged -= e);
    public static IEventObservable<DoubleUserInputInfo> LabelBObservable => field ??= Observable.ForEvent<DoubleUserInputInfo>((s, e) => s.LabelBChanged += e, (s, e) => s.LabelBChanged -= e);
    public static IEventObservable<DoubleUserInputInfo> LineCountHintAObservable => field ??= Observable.ForEvent<DoubleUserInputInfo>((s, e) => s.LineCountHintAChanged += e, (s, e) => s.LineCountHintAChanged -= e);
    public static IEventObservable<DoubleUserInputInfo> LineCountHintBObservable => field ??= Observable.ForEvent<DoubleUserInputInfo>((s, e) => s.LineCountHintBChanged += e, (s, e) => s.LineCountHintBChanged -= e);
    public static IEventObservable<DoubleUserInputInfo> TextErrorsAObservable => field ??= Observable.ForEvent<DoubleUserInputInfo>((s, e) => s.TextErrorsAChanged += e, (s, e) => s.TextErrorsAChanged -= e);
    public static IEventObservable<DoubleUserInputInfo> TextErrorsBObservable => field ??= Observable.ForEvent<DoubleUserInputInfo>((s, e) => s.TextErrorsBChanged += e, (s, e) => s.TextErrorsBChanged -= e);
    public static IEventObservable<DoubleUserInputInfo> DebounceErrorsDelayAObservable => field ??= Observable.ForEvent<DoubleUserInputInfo>((s, e) => s.DebounceErrorsDelayAChanged += e, (s, e) => s.DebounceErrorsDelayAChanged -= e);
    public static IEventObservable<DoubleUserInputInfo> DebounceErrorsDelayBObservable => field ??= Observable.ForEvent<DoubleUserInputInfo>((s, e) => s.DebounceErrorsDelayBChanged += e, (s, e) => s.DebounceErrorsDelayBChanged -= e);
    public static IEventObservable<DoubleUserInputInfo> DebounceAElapsedObservable => field ??= Observable.ForEvent<DoubleUserInputInfo>((s, e) => s.DebounceAElapsed += e, (s, e) => s.DebounceAElapsed -= e);
    public static IEventObservable<DoubleUserInputInfo> DebounceBElapsedObservable => field ??= Observable.ForEvent<DoubleUserInputInfo>((s, e) => s.DebounceBElapsed += e, (s, e) => s.DebounceBElapsed -= e);
    
    private static readonly SendOrPostCallback s_UpdateTextErrorsAForced = static x => ((DoubleUserInputInfo) x!).UpdateTextAError(true);
    private static readonly SendOrPostCallback s_UpdateTextErrorsBForced = static x => ((DoubleUserInputInfo) x!).UpdateTextBError(true);
    
    private string textA = "", textB = "";
    private bool isUpdatingErrorA, isUpdatingErrorB;
    private bool doUpdateBAfterA, doUpdateAAfterB;
    private TimerDispatcherDebouncer? errorDebouncerA, errorDebouncerB;

    /// <summary>
    /// Gets or sets the text in the A field
    /// </summary>
    public string TextA {
        get => this.textA;
        set {
            value = SingleUserInputInfo.CanonicalizeTextForLineCount(value, this.LineCountHintA);
            PropertyHelper.SetAndRaiseINE(ref this.textA, value, this, static t => {
                SingleUserInputInfo.HandleTextChanged(s_UpdateTextErrorsAForced, t, t.DebounceErrorsDelayA, ref t.errorDebouncerA, t.ValidateA);
                t.TextAChanged?.Invoke(t, EventArgs.Empty);
            });
        }
    }

    /// <summary>
    /// Gets or sets the text in the B field
    /// </summary>
    public string TextB {
        get => this.textB;
        set {
            value = SingleUserInputInfo.CanonicalizeTextForLineCount(value, this.LineCountHintB);
            PropertyHelper.SetAndRaiseINE(ref this.textB, value, this, static t => {
                SingleUserInputInfo.HandleTextChanged(s_UpdateTextErrorsBForced, t, t.DebounceErrorsDelayB, ref t.errorDebouncerB, t.ValidateB);
                t.TextBChanged?.Invoke(t, EventArgs.Empty);
            });
        }
    }

    /// <summary>
    /// Gets or sets the text displayed above the <see cref="TextA"/> text field
    /// </summary>
    public string? LabelA {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.LabelAChanged);
    }

    /// <summary>
    /// Gets or sets the text displayed above the <see cref="TextB"/> text field
    /// </summary>
    public string? LabelB {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.LabelBChanged);
    }

    public string? PrefixA {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.PrefixAChanged);
    }

    public string? PrefixB {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.PrefixBChanged);
    }
    
    /// <summary>
    /// Gets or sets a hint for the amount of visual lines the text input A should display. 
    /// Default is 1. A value greater than 1 disables auto-close when pressing return
    /// </summary>
    public int LineCountHintA {
        get => field;
        set {
            if (value < 1)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value cannot be less than 1");
            PropertyHelper.SetAndRaiseINE(ref field, value, this, this.LineCountHintAChanged);
        }
    } = 1;

    /// <summary>
    /// Gets or sets a hint for the amount of visual lines the text input B should display. 
    /// Default is 1. A value greater than 1 disables auto-close when pressing return
    /// </summary>
    public int LineCountHintB {
        get => field;
        set {
            if (value < 1)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value cannot be less than 1");
            PropertyHelper.SetAndRaiseINE(ref field, value, this, this.LineCountHintBChanged);
        }
    } = 1;

    /// <summary>
    /// A validation callback for <see cref="TextA"/>
    /// </summary>
    public Action<ValidationArgs>? ValidateA { get; init; }

    /// <summary>
    /// A validation callback for <see cref="TextB"/>
    /// </summary>
    public Action<ValidationArgs>? ValidateB { get; init; }

    /// <summary>
    /// Gets the error messages with <see cref="TextA"/>. Only non-null
    /// when <see cref="ValidateA"/> is non-null and actually produces errors
    /// </summary>
    public ReadOnlyCollection<string>? TextErrorsA {
        get => field;
        private set {
            if (value?.Count < 1) {
                value = null; // set empty to null for simplified usage of the property
            }

            if (!ReferenceEquals(field, value)) {
                field = value;
                this.TextErrorsAChanged?.Invoke(this, EventArgs.Empty);
                this.RaiseHasErrorsChanged();
            }
        }
    }

    /// <summary>
    /// Gets the error messages with <see cref="TextB"/>. Only non-null
    /// when <see cref="ValidateB"/> is non-null and actually produces errors
    /// </summary>
    public ReadOnlyCollection<string>? TextErrorsB {
        get => field;
        private set {
            if (value?.Count < 1) {
                value = null; // set empty to null for simplified usage of the property
            }

            if (!ReferenceEquals(field, value)) {
                field = value;
                this.TextErrorsBChanged?.Invoke(this, EventArgs.Empty);
                this.RaiseHasErrorsChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the amount of milliseconds to wait before actually querying <see cref="ValidateA"/>. Default is 0, meaning do not wait.
    /// </summary>
    public int DebounceErrorsDelayA {
        get => field;
        set {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value cannot be negative");
            if (field == value)
                return;

            field = value;
            SingleUserInputInfo.HandleDebounceDelayChanged(s_UpdateTextErrorsAForced, this, value, ref this.errorDebouncerA);
            this.DebounceErrorsDelayAChanged?.Invoke(this, EventArgs.Empty);
        }
    }


    /// <summary>
    /// Gets or sets the amount of milliseconds to wait before actually querying <see cref="ValidateB"/>. Default is 0, meaning do not wait.
    /// </summary>
    public int DebounceErrorsDelayB {
        get => field;
        set {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value cannot be negative");
            if (field == value)
                return;

            field = value;
            SingleUserInputInfo.HandleDebounceDelayChanged(s_UpdateTextErrorsBForced, this, value, ref this.errorDebouncerB);
            this.DebounceErrorsDelayBChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? TextAChanged, TextBChanged;
    public event EventHandler? LabelAChanged, LabelBChanged;
    public event EventHandler? PrefixBChanged, PrefixAChanged;
    public event EventHandler? LineCountHintAChanged, LineCountHintBChanged;
    public event EventHandler? TextErrorsAChanged, TextErrorsBChanged;
    public event EventHandler? DebounceErrorsDelayAChanged, DebounceErrorsDelayBChanged;
    
    /// <summary>
    /// An event raised when the debounce delay A has elapsed and <see cref="TextErrorsA"/> has been updated.
    /// If <see cref="DebounceErrorsDelayA"/> is zero, then this event is raised immediately.
    /// </summary>
    public event EventHandler? DebounceAElapsed;
    
    /// <summary>
    /// An event raised when the debounce delay A has elapsed and <see cref="TextErrorsB"/> has been updated.
    /// If <see cref="DebounceErrorsDelayB"/> is zero, then this event is raised immediately.
    /// </summary>
    public event EventHandler? DebounceBElapsed;

    public DoubleUserInputInfo() {
    }

    public DoubleUserInputInfo(string? textA, string? textB) {
        this.textA = textA ?? "";
        this.textB = textB ?? "";
    }

    /// <summary>
    /// Updates the errors using <see cref="TextA"/>. Invoke this if the validator relies on the value
    /// of <see cref="TextB"/>  and it changes such that it effectively invalidates <see cref="TextA"/>
    /// </summary>
    /// <param name="force">
    /// Immediately call <see cref="ValidateA"/>. Using true may result in stack overflow exception
    /// if calling from <see cref="ValidateB"/>, depending on how one value depends on another
    /// </param>
    public void UpdateTextAError(bool force) {
        this.errorDebouncerA?.Reset();
        
        if (force) {
            if (this.isUpdatingErrorA) {
                return;
            }
            
            this.isUpdatingErrorA = true;
            this.TextErrorsA = SingleUserInputInfo.GetErrors(this.TextA, this.ValidateA, this.TextErrorsA != null)?.AsReadOnly();
            this.DebounceAElapsed?.Invoke(this, EventArgs.Empty);
            
            if (this.doUpdateBAfterA) {
                this.doUpdateBAfterA = false;
                this.UpdateTextBError(true);
            }
            
            this.isUpdatingErrorA = false;
        }
        else if (!this.isUpdatingErrorB)
            throw new InvalidCastException("Cannot use " + nameof(force) + "=false when not updating " + nameof(this.ValidateB));
        else this.doUpdateAAfterB = true;
    }

    /// <summary>
    /// Updates the errors using <see cref="TextB"/>. Invoke this if the validator relies on the value
    /// of <see cref="TextA"/>  and it changes such that it effectively invalidates <see cref="TextB"/>
    /// </summary>
    /// <param name="force">
    /// Immediately call <see cref="ValidateB"/>. Using true may result in stack overflow exception
    /// if calling from <see cref="ValidateA"/>, depending on how one value depends on another
    /// </param>
    public void UpdateTextBError(bool force) {
        this.errorDebouncerB?.Reset();
        
        if (force) {
            if (this.isUpdatingErrorB) {
                return;
            }
            
            this.isUpdatingErrorB = true;
            this.TextErrorsB = SingleUserInputInfo.GetErrors(this.TextB, this.ValidateB, this.TextErrorsB != null)?.AsReadOnly();
            this.DebounceBElapsed?.Invoke(this, EventArgs.Empty);
            
            if (this.doUpdateAAfterB) {
                this.doUpdateAAfterB = false;
                this.UpdateTextAError(true);
            }
            
            this.isUpdatingErrorB = false;
        }
        else if (!this.isUpdatingErrorA)
            throw new InvalidCastException("Cannot use " + nameof(force) + "=false when not updating " + nameof(this.ValidateA));
        else this.doUpdateBAfterA = true;
    }

    public override bool HasErrors() => this.TextErrorsA != null || this.TextErrorsB != null;

    public override void UpdateAllErrors() {
        this.UpdateTextAError(true);
        this.UpdateTextBError(true);
    }
}