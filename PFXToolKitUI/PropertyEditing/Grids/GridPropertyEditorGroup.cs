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

using PFXToolKitUI.Utils;

namespace PFXToolKitUI.PropertyEditing.Grids;

public delegate void GridPropertyEditorGroupEventHandler(GridPropertyEditorGroup sender);

public class GridPropertyEditorGroup : SimplePropertyEditorGroup {
    private readonly Dictionary<BasePropertyEditorObject, (int, int)> PropObjLocation;
    private List<GridColumnDefinition> columns = new List<GridColumnDefinition>();
    private List<GridRowDefinition> rows = new List<GridRowDefinition>();

    public List<GridColumnDefinition> Columns {
        get => this.columns;
        set => PropertyHelper.SetAndRaiseINE(ref this.columns, value, this, static t => t.ColumnsChanged?.Invoke(t));
    }

    public List<GridRowDefinition> Rows {
        get => this.rows;
        set => PropertyHelper.SetAndRaiseINE(ref this.rows, value, this, static t => t.RowsChanged?.Invoke(t));
    }

    public event GridPropertyEditorGroupEventHandler? ColumnsChanged;
    public event GridPropertyEditorGroupEventHandler? RowsChanged;

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