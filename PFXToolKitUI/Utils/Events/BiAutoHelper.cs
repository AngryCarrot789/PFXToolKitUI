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

namespace PFXToolKitUI.Utils.Events;

public class BiAutoHelper<TA, TB> where TA : class where TB : class {
    public delegate void BiAutoHelperValueAChangedEventHandler(BiAutoHelper<TA, TB> sender, TA? oldValueA, TA? newValueA);
    public delegate void BiAutoHelperValueBChangedEventHandler(BiAutoHelper<TA, TB> sender, TB? oldValueB, TB? newValueB);
    
    private readonly Action<TA, TB>? onEnabled;
    private readonly Action<TA, TB>? onDisabled;

    private TA? valueA;
    private TB? valueB;

    public TA? ValueA {
        get => this.valueA;
        set {
            if (this.valueA != null && this.valueB != null)
                this.onDisabled?.Invoke(this.valueA, this.valueB);
            
            PropertyHelper.SetAndRaiseINE(ref this.valueA, value, this, static (t, o, n) => t.ValueAChanged?.Invoke(t, o, n));
            
            if (value != null && this.valueB != null)
                this.onEnabled?.Invoke(value, this.valueB);
        }
    }

    public TB? ValueB {
        get => this.valueB;
        set {
            if (this.valueA != null && this.valueB != null)
                this.onDisabled?.Invoke(this.valueA, this.valueB);
            
            PropertyHelper.SetAndRaiseINE(ref this.valueB, value, this, static (t, o, n) => t.ValueBChanged?.Invoke(t, o, n));
            
            if (this.valueA != null && value != null)
                this.onEnabled?.Invoke(this.valueA, value);
        }
    }

    public event BiAutoHelperValueAChangedEventHandler? ValueAChanged;
    public event BiAutoHelperValueBChangedEventHandler? ValueBChanged;
    
    public BiAutoHelper(Action<TA, TB> onEnabled, Action<TA, TB> onDisabled) {
        this.onEnabled = onEnabled;
        this.onDisabled = onDisabled;
    }
}