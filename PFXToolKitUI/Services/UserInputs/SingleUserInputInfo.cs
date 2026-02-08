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

public class SingleUserInputInfo : BaseTextUserInputInfo {
    private static readonly SendOrPostCallback s_UpdateTextErrors = static x => ((SingleUserInputInfo) x!).UpdateTextError();

    public static IEventObservable<SingleUserInputInfo> TextObservable => field ??= Observable.ForEvent<SingleUserInputInfo>((s, e) => s.TextChanged += e, (s, e) => s.TextChanged -= e);
    public static IEventObservable<SingleUserInputInfo> LabelObservable => field ??= Observable.ForEvent<SingleUserInputInfo>((s, e) => s.LabelChanged += e, (s, e) => s.LabelChanged -= e);
    public static IEventObservable<SingleUserInputInfo> TextErrorsObservable => field ??= Observable.ForEvent<SingleUserInputInfo>((s, e) => s.TextErrorsChanged += e, (s, e) => s.TextErrorsChanged -= e);
    public static IEventObservable<SingleUserInputInfo> LineCountHintObservable => field ??= Observable.ForEvent<SingleUserInputInfo>((s, e) => s.LineCountHintChanged += e, (s, e) => s.LineCountHintChanged -= e);
    public static IEventObservable<SingleUserInputInfo> MinimumDialogWidthHintObservable => field ??= Observable.ForEvent<SingleUserInputInfo>((s, e) => s.MinimumDialogWidthHintChanged += e, (s, e) => s.MinimumDialogWidthHintChanged -= e);
    public static IEventObservable<SingleUserInputInfo> DebounceErrorsDelayObservable => field ??= Observable.ForEvent<SingleUserInputInfo>((s, e) => s.DebounceErrorsDelayChanged += e, (s, e) => s.DebounceErrorsDelayChanged -= e);

    private string text;
    private string? label;
    private TimerDispatcherDebouncer? errorDebouncer;

    /// <summary>
    /// Gets the value the user have typed into the text field
    /// </summary>
    public string Text {
        get => this.text;
        set {
            value = CanonicalizeTextForLineCount(value, this.LineCountHint);
            PropertyHelper.SetAndRaiseINE(ref this.text, value, this, static t => {
                HandleTextChanged(s_UpdateTextErrors, t, t.DebounceErrorsDelay, ref t.errorDebouncer, t.Validate);
                t.TextChanged?.Invoke(t, EventArgs.Empty);
            });
        }
    }

    /// <summary>
    /// Gets the label placed right above the text field
    /// </summary>
    public string? Label {
        get => this.label;
        set => PropertyHelper.SetAndRaiseINE(ref this.label, value, this, this.LabelChanged);
    }

    /// <summary>
    /// Gets or sets a hint for the amount of visual lines the text input should display. Default is 1,
    /// meaning only 1 line is shown. A value greater than 1 disables auto-close when pressing return.
    /// <para>
    /// Note, when this value is 1, <see cref="Text"/> will be canonicalized to remove <c>\n</c> and <c>\r</c> characters 
    /// </para>
    /// </summary>
    public int LineCountHint {
        get => field;
        set {
            if (value < 1)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value cannot be less than 1");
            PropertyHelper.SetAndRaiseINE(ref field, value, this, this.LineCountHintChanged);
        }
    } = 1;

    /// <summary>
    /// Gets or sets a hint for the minimum width of the dialog. Default is -1, meaning no hint
    /// </summary>
    public int MinimumDialogWidthHint {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.MinimumDialogWidthHintChanged);
    } = -1;

    /// <summary>
    /// A validation function that is given the current text and a list. If there's problems
    /// with the text, then error messages should be added to the list. 
    /// </summary>
    public Action<ValidationArgs>? Validate { get; init; }

    /// <summary>
    /// Gets the current list of errors present. This value will either be null, or it will have at least one element
    /// </summary>
    public ReadOnlyCollection<string>? TextErrors {
        get => field;
        private set {
            if (value?.Count < 1) {
                value = null; // set empty to null for simplified usage of the property
            }

            if (!ReferenceEquals(field, value)) {
                field = value;
                this.TextErrorsChanged?.Invoke(this, EventArgs.Empty);
                this.RaiseHasErrorsChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the amount of milliseconds to wait before actually querying <see cref="ValidateA"/>.
    /// Default is 0, meaning do not wait. This property can be used for when <see cref="Validate"/> is expensive.
    /// For example, it parses expressions, and you don't want it parsing until the user has stopped typing for some time.
    /// </summary>
    public int DebounceErrorsDelay {
        get => field;
        set {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value cannot be negative");
            if (field == value)
                return;

            HandleDebounceDelayChanged(s_UpdateTextErrors, this, value, ref this.errorDebouncer);
            field = value;
            this.DebounceErrorsDelayChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? TextChanged;
    public event EventHandler? LabelChanged;
    public event EventHandler? TextErrorsChanged;
    public event EventHandler? LineCountHintChanged;
    public event EventHandler? MinimumDialogWidthHintChanged;
    public event EventHandler? DebounceErrorsDelayChanged;

    public SingleUserInputInfo(string? defaultText) : this(null, null, null, defaultText) {
    }

    public SingleUserInputInfo(string? caption, string? label, string? defaultText) : this(caption, null, label, defaultText) {
    }

    public SingleUserInputInfo(string? caption, string? message, string? label, string? defaultText) : base(caption, message) {
        this.label = label;
        this.text = defaultText ?? "";
    }

    private void UpdateTextError() {
        this.errorDebouncer?.Reset();
        this.TextErrors = GetErrors(this.Text, this.Validate, this.HasErrors())?.AsReadOnly();
    }

    public override bool HasErrors() => this.TextErrors != null;

    public override void UpdateAllErrors() {
        this.UpdateTextError();
    }

    public static List<string>? GetErrors(string text, Action<ValidationArgs>? validate, bool hasError) {
        if (validate == null) {
            return null;
        }

        List<string> list = new List<string>();
        validate(new ValidationArgs(text, list, hasError));
        return list.Count > 0 ? list : null;
    }

    public static void HandleTextChanged(SendOrPostCallback updateErrors, object state, int debounceDelay, ref TimerDispatcherDebouncer? debouncer, Action<ValidationArgs>? validate) {
        if (debouncer == null && debounceDelay > 0 && validate != null)
            debouncer = new TimerDispatcherDebouncer(TimeSpan.FromMilliseconds(debounceDelay), updateErrors, state);

        if (debouncer != null && validate != null) {
            debouncer.TryInvokeOrPostpone();
        }
        else {
            // validate function might have been set to null at some point,
            // so reset debouncer so we don't update later for no reason
            debouncer?.Reset();
            updateErrors(state);
        }
    }

    public static void HandleDebounceDelayChanged(SendOrPostCallback updateErrors, object state, int newValue, ref TimerDispatcherDebouncer? debouncer) {
        if (debouncer != null) {
            if (newValue == 0) {
                if (debouncer.IsWaiting)
                    ApplicationPFX.Instance.Dispatcher.Post(updateErrors, state);

                debouncer.Reset();
                debouncer = null;
            }
            else {
                debouncer.Interval = TimeSpan.FromMilliseconds(newValue);
            }
        }
    }

    public static string CanonicalizeTextForLineCount(string value, int lineCount) {
        if (lineCount == 1) {
            int idx = value.IndexOf("\r\n");
            if (idx == -1) {
                idx = value.IndexOf('\n');
                if (idx == -1)
                    idx = value.IndexOf('\r');
            }
            
            if (idx >= 0)
                value = value.Substring(0, idx);
        }
        
        return value;
    }
}