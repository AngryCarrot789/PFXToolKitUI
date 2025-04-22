// 
// Copyright (c) 2024-2025 REghZy
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

using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Interactivity.Formatting;

namespace PFXToolKitUI.PropertyEditing.DataTransfer;

public delegate void DataParameterValueFormatterChangedEventHandler(DataParameterFormattablePropertyEditorSlot sender, IValueFormatter? oldValueFormatter, IValueFormatter? newValueFormatter);

public abstract class DataParameterFormattablePropertyEditorSlot : DataParameterPropertyEditorSlot {
    private IValueFormatter? valueFormatter;
    
    /// <summary>
    /// Gets or sets the value formatter used to format our numeric value in the UI
    /// </summary>
    public IValueFormatter? ValueFormatter {
        get => this.valueFormatter;
        set {
            IValueFormatter? oldValueFormatter = this.valueFormatter;
            if (oldValueFormatter == value)
                return;

            this.valueFormatter = value;
            this.ValueFormatterChanged?.Invoke(this, oldValueFormatter, value);
        }
    }
    
    public event DataParameterValueFormatterChangedEventHandler? ValueFormatterChanged;
    
    public DataParameterFormattablePropertyEditorSlot(DataParameter parameter, Type applicableType, string? displayName = null) : base(parameter, applicableType, displayName) {
    }
}