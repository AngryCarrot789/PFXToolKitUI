// 
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

public delegate void DoubleUserInputDataEventHandler(DoubleUserInputInfo sender);

public class DoubleUserInputInfo : BaseTextUserInputInfo {
    public static readonly DataParameterString TextAParameter = DataParameter.Register(new DataParameterString(typeof(DoubleUserInputInfo), nameof(TextA), "", ValueAccessors.Reflective<string?>(typeof(DoubleUserInputInfo), nameof(textA)), isNullable: false));
    public static readonly DataParameterString TextBParameter = DataParameter.Register(new DataParameterString(typeof(DoubleUserInputInfo), nameof(TextB), "", ValueAccessors.Reflective<string?>(typeof(DoubleUserInputInfo), nameof(textB)), isNullable: false));
    public static readonly DataParameterString LabelAParameter = DataParameter.Register(new DataParameterString(typeof(DoubleUserInputInfo), nameof(LabelA), null, ValueAccessors.Reflective<string?>(typeof(DoubleUserInputInfo), nameof(labelA))));
    public static readonly DataParameterString LabelBParameter = DataParameter.Register(new DataParameterString(typeof(DoubleUserInputInfo), nameof(LabelB), null, ValueAccessors.Reflective<string?>(typeof(DoubleUserInputInfo), nameof(labelB))));

    private string textA = TextAParameter.DefaultValue!;
    private string textB = TextBParameter.DefaultValue!;
    private string? labelA = LabelAParameter.DefaultValue;
    private string? labelB = LabelBParameter.DefaultValue;
    private IImmutableList<string>? textErrorsA, textErrorsB;
    private bool isUpdatingErrorA, isUpdatingErrorB;
    private bool doUpdateBAfterA, doUpdateAAfterB;

    public string TextA {
        get => this.textA;
        set => DataParameter.SetValueHelper<string?>(this, TextAParameter, ref this.textA!, value ?? "");
    }

    public string TextB {
        get => this.textB;
        set => DataParameter.SetValueHelper<string?>(this, TextBParameter, ref this.textB!, value ?? "");
    }

    public string? LabelA {
        get => this.labelA;
        set => DataParameter.SetValueHelper(this, LabelAParameter, ref this.labelA, value);
    }

    public string? LabelB {
        get => this.labelB;
        set => DataParameter.SetValueHelper(this, LabelBParameter, ref this.labelB, value);
    }

    public Action<ValidationArgs>? ValidateA { get; set; }

    public Action<ValidationArgs>? ValidateB { get; set; }

    public IImmutableList<string>? TextErrorsA {
        get => this.textErrorsA;
        private set {
            if (value?.Count < 1) {
                value = null; // set empty to null for simplified usage of the property
            }

            if (!ReferenceEquals(this.textErrorsA, value)) {
                this.textErrorsA = value;
                this.TextErrorsAChanged?.Invoke(this);
                this.RaiseHasErrorsChanged();
            }
        }
    }

    public IImmutableList<string>? TextErrorsB {
        get => this.textErrorsB;
        private set {
            if (value?.Count < 1) {
                value = null; // set empty to null for simplified usage of the property
            }

            if (!ReferenceEquals(this.textErrorsB, value)) {
                this.textErrorsB = value;
                this.TextErrorsBChanged?.Invoke(this);
                this.RaiseHasErrorsChanged();
            }
        }
    }

    public event DoubleUserInputDataEventHandler? TextErrorsAChanged;
    public event DoubleUserInputDataEventHandler? TextErrorsBChanged;

    public DoubleUserInputInfo() {
    }

    public DoubleUserInputInfo(string? textA, string? textB) {
        this.textA = textA ?? "";
        this.textB = textB ?? "";
    }

    static DoubleUserInputInfo() {
        TextAParameter.PriorityValueChanged += (p, o) => ((DoubleUserInputInfo) o).UpdateTextAError(true);
        TextBParameter.PriorityValueChanged += (p, o) => ((DoubleUserInputInfo) o).UpdateTextBError(true);
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
        if (force) {
            if (this.isUpdatingErrorA) {
                return;
            }
            
            this.isUpdatingErrorA = true;
            this.TextErrorsA = SingleUserInputInfo.GetErrors(this.TextA, this.ValidateA, this.TextErrorsA != null);
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
        if (force) {
            if (this.isUpdatingErrorB) {
                return;
            }
            
            this.isUpdatingErrorB = true;
            this.TextErrorsB = SingleUserInputInfo.GetErrors(this.TextB, this.ValidateB, this.TextErrorsB != null);
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