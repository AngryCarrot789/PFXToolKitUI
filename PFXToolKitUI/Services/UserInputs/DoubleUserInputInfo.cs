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
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Services.UserInputs;

public delegate void DoubleUserInputDataEventHandler(DoubleUserInputInfo sender);

public class DoubleUserInputInfo : BaseTextUserInputInfo {
    private string textA, textB = "";
    private string? labelA, labelB;
    private ReadOnlyCollection<string>? textErrorsA, textErrorsB;
    private bool isUpdatingErrorA, isUpdatingErrorB;
    private bool doUpdateBAfterA, doUpdateAAfterB;

    /// <summary>
    /// Gets or sets the text in the A field
    /// </summary>
    public string TextA {
        get => this.textA;
        set => PropertyHelper.SetAndRaiseINE(ref this.textA, value, this, static t => {
            t.UpdateTextAError(true);
            t.TextAChanged?.Invoke(t);
        });
    }
    
    /// <summary>
    /// Gets or sets the text in the B field
    /// </summary>
    public string TextB {
        get => this.textB;
        set => PropertyHelper.SetAndRaiseINE(ref this.textB, value, this, static t => {
            t.UpdateTextBError(true);
            t.TextBChanged?.Invoke(t);
        });
    }

    /// <summary>
    /// Gets or sets the text displayed above the <see cref="TextA"/> text field
    /// </summary>
    public string? LabelA {
        get => this.labelA;
        set => PropertyHelper.SetAndRaiseINE(ref this.labelA, value, this, static t => t.LabelAChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets the text displayed above the <see cref="TextB"/> text field
    /// </summary>
    public string? LabelB {
        get => this.labelB;
        set => PropertyHelper.SetAndRaiseINE(ref this.labelB, value, this, static t => t.LabelBChanged?.Invoke(t));
    }

    /// <summary>
    /// A validation callback for <see cref="TextA"/>
    /// </summary>
    public Action<ValidationArgs>? ValidateA { get; set; }

    /// <summary>
    /// A validation callback for <see cref="TextB"/>
    /// </summary>
    public Action<ValidationArgs>? ValidateB { get; set; }

    /// <summary>
    /// Gets the error messages with <see cref="TextA"/>. Only non-null
    /// when <see cref="ValidateA"/> is non-null and actually produces errors
    /// </summary>
    public ReadOnlyCollection<string>? TextErrorsA {
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

    /// <summary>
    /// Gets the error messages with <see cref="TextB"/>. Only non-null
    /// when <see cref="ValidateB"/> is non-null and actually produces errors
    /// </summary>
    public ReadOnlyCollection<string>? TextErrorsB {
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

    public event DoubleUserInputDataEventHandler? TextAChanged, TextBChanged, LabelAChanged, LabelBChanged;
    public event DoubleUserInputDataEventHandler? TextErrorsAChanged, TextErrorsBChanged;

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
        if (force) {
            if (this.isUpdatingErrorA) {
                return;
            }
            
            this.isUpdatingErrorA = true;
            this.TextErrorsA = SingleUserInputInfo.GetErrors(this.TextA, this.ValidateA, this.TextErrorsA != null)?.AsReadOnly();
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
            this.TextErrorsB = SingleUserInputInfo.GetErrors(this.TextB, this.ValidateB, this.TextErrorsB != null)?.AsReadOnly();
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