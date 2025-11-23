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

using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.EventHelpers;

public class BiAutoHelper<TA, TB> where TA : class where TB : class {
    private readonly Action<TA, TB>? onEnabled;
    private readonly Action<TA, TB>? onDisabled;

    private TA? valueA;
    private TB? valueB;

    public TA? ValueA {
        get => this.valueA;
        set {
            if (this.valueA != null && this.valueB != null)
                this.onDisabled?.Invoke(this.valueA, this.valueB);
            
            PropertyHelper.SetAndRaiseINE(ref this.valueA, value, this, this.ValueAChanged);
            
            if (value != null && this.valueB != null)
                this.onEnabled?.Invoke(value, this.valueB);
        }
    }

    public TB? ValueB {
        get => this.valueB;
        set {
            if (this.valueA != null && this.valueB != null)
                this.onDisabled?.Invoke(this.valueA, this.valueB);
            
            PropertyHelper.SetAndRaiseINE(ref this.valueB, value, this, this.ValueBChanged);
            
            if (this.valueA != null && value != null)
                this.onEnabled?.Invoke(this.valueA, value);
        }
    }

    public event EventHandler? ValueAChanged;
    public event EventHandler? ValueBChanged;
    
    public BiAutoHelper(Action<TA, TB> onEnabled, Action<TA, TB> onDisabled) {
        this.onEnabled = onEnabled;
        this.onDisabled = onDisabled;
    }
}