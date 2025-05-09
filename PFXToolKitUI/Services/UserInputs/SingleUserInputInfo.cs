﻿// 
// Copyright (c) 2023-2025 REghZy
// 
// This file is part of FramePFX.
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
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System.Collections.Immutable;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Utils.Accessing;

namespace PFXToolKitUI.Services.UserInputs;

public delegate void SingleUserInputDataEventHandler(SingleUserInputInfo sender);

public class SingleUserInputInfo : BaseTextUserInputInfo {
    public static readonly DataParameterString TextParameter = DataParameter.Register(new DataParameterString(typeof(SingleUserInputInfo), nameof(Text), "", ValueAccessors.Reflective<string?>(typeof(SingleUserInputInfo), nameof(text)), isNullable: false));
    public static readonly DataParameterString LabelParameter = DataParameter.Register(new DataParameterString(typeof(SingleUserInputInfo), nameof(Label), null, ValueAccessors.Reflective<string?>(typeof(SingleUserInputInfo), nameof(label))));

    private string text;
    private string? label = LabelParameter.DefaultValue;
    private IImmutableList<string>? textErrors;

    /// <summary>
    /// Gets the value the user have typed into the text field
    /// </summary>
    public string Text {
        get => this.text;
        set => DataParameter.SetValueHelper<string?>(this, TextParameter, ref this.text!, value ?? "");
    }

    /// <summary>
    /// Gets the label placed right above the text field
    /// </summary>
    public string? Label {
        get => this.label;
        set => DataParameter.SetValueHelper(this, LabelParameter, ref this.label, value);
    }

    /// <summary>
    /// A validation function that is given the current text and a list. If there's problems
    /// with the text, then error messages should be added to the list. 
    /// </summary>
    public Action<ValidationArgs>? Validate { get; set; }

    /// <summary>
    /// Gets the current list of errors present. This value will either be null, or it will have at least one element
    /// </summary>
    public IImmutableList<string>? TextErrors {
        get => this.textErrors;
        private set {
            if (value?.Count < 1) {
                value = null; // set empty to null for simplified usage of the property
            }

            if (!ReferenceEquals(this.textErrors, value)) {
                this.textErrors = value;
                this.TextErrorsChanged?.Invoke(this);
                this.RaiseHasErrorsChanged();
            }
        }
    }

    public event SingleUserInputDataEventHandler? TextErrorsChanged;

    public SingleUserInputInfo(string? defaultText) : this(null, null, null, defaultText) {
    }

    public SingleUserInputInfo(string? caption, string? label, string? defaultText) : this(caption, null, label, defaultText) {
    }

    public SingleUserInputInfo(string? caption, string? message, string? label, string? defaultText) : base(caption, message) {
        this.label = label;
        this.text = defaultText ?? "";
        this.TransferableData.AddValueChangedHandler(TextParameter, (p, o) => ((SingleUserInputInfo) o).UpdateTextError());
    }
    
    static SingleUserInputInfo() {
        TextParameter.PriorityValueChanged += (p, o) => ((SingleUserInputInfo) o).UpdateTextError();
    }

    private void UpdateTextError() {
        this.TextErrors = GetErrors(this.Text, this.Validate, this.HasErrors());
    }

    public override bool HasErrors() => this.TextErrors != null;

    public override void UpdateAllErrors() {
        this.UpdateTextError();
    }

    public static ImmutableList<string>? GetErrors(string text, Action<ValidationArgs>? validate, bool hasError) {
        if (validate == null) {
            return null;
        }

        List<string> list = new List<string>();
        validate(new ValidationArgs(text, list, hasError));
        return list.Count > 0 ? list.ToImmutableList() : null;
    }
}