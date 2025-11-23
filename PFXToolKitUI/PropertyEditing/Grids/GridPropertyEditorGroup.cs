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

using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.PropertyEditing.Grids;

public class GridPropertyEditorGroup : SimplePropertyEditorGroup {
    private readonly Dictionary<BasePropertyEditorObject, (int, int)> PropObjLocation;

    public List<GridColumnDefinition> Columns {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.ColumnsChanged);
    } = new List<GridColumnDefinition>();

    public List<GridRowDefinition> Rows {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.RowsChanged);
    } = new List<GridRowDefinition>();

    public event EventHandler? ColumnsChanged;
    public event EventHandler? RowsChanged;

    public GridPropertyEditorGroup(Type applicableType, GroupType groupType = GroupType.NoExpander) : base(applicableType, groupType) {
        this.PropObjLocation = new Dictionary<BasePropertyEditorObject, (int, int)>();
    }

    public bool GetLocation(BasePropertyEditorObject propObj, out int column, out int row) {
        if (this.PropObjLocation.TryGetValue(propObj, out (int column, int row) x)) {
            column = x.column;
            row = x.row;
            return true;
        }
        else {
            column = row = default;
            return false;
        }
    }

    public void SetItemLocation(BasePropertyEditorObject propObj, int column = 0, int row = 0) {
        this.PropObjLocation[propObj] = (column, row);
    }

    public void InsertItem(int index, BasePropertyEditorObject propObj, int column = 0, int row = 0) {
        this.SetItemLocation(propObj, column, row);
        base.InsertItem(index, propObj);
    }

    public override void RemoveItemAt(int index) {
        this.PropObjLocation.Remove(base.PropertyObjects[index]);
        base.RemoveItemAt(index);
    }
}

public abstract class BaseGridDefinition {
}

public class GridColumnDefinition : BaseGridDefinition {
    public GridDefinitionSize Width { get; }

    public GridColumnDefinition(GridDefinitionSize width) {
        this.Width = width;
    }
}

public class GridRowDefinition : BaseGridDefinition {
    public GridDefinitionSize Height { get; }

    public GridRowDefinition(GridDefinitionSize height) {
        this.Height = height;
    }
}

public readonly struct GridDefinitionSize {
    public readonly double Value;
    public readonly GridSizeType SizeType;

    public GridDefinitionSize(double value, GridSizeType sizeType = GridSizeType.Pixel) {
        this.SizeType = sizeType;
        this.Value = value;
    }
}

public enum GridSizeType {
    /// <summary>The row or column is auto-sized to fit its content.</summary>
    Auto,

    /// <summary>
    /// The row or column is sized in device independent pixels.
    /// </summary>
    Pixel,

    /// <summary>
    /// The row or column is sized as a weighted proportion of available space.
    /// </summary>
    Star,
}