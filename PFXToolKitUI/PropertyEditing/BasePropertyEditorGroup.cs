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
using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.PropertyEditing;

public abstract class BasePropertyEditorGroup : BasePropertyEditorItem {
    private readonly List<BasePropertyEditorObject> propObjs;

    /// <summary>
    /// Gets a read-only collection that contains all of our child <see cref="BasePropertyEditorObject"/> objects
    /// </summary>
    public ReadOnlyCollection<BasePropertyEditorObject> PropertyObjects { get; }

    /// <summary>
    /// Gets or sets this group's display name
    /// </summary>
    public string? DisplayName {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.DisplayNameChanged);
    }

    /// <summary>
    /// Gets or sets if this group is expanded or not
    /// </summary>
    public bool IsExpanded {
        get => field;
        set {
            if (field == value)
                return;
            field = value;
            this.IsExpandedChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    /// <summary>
    /// Gets the group type. This should not change during the lifetime of this instance
    /// </summary>
    public GroupType GroupType { get; }

    public bool IsRoot => this.Parent == null!;

    public event EventHandler<ItemAddOrRemoveEventArgs<BasePropertyEditorObject>>? ItemAdded;
    public event EventHandler<ItemAddOrRemoveEventArgs<BasePropertyEditorObject>>? ItemRemoved;
    public event EventHandler<ItemMoveEventArgs<BasePropertyEditorObject>>? ItemMoved;
    public event EventHandler? DisplayNameChanged;
    public event EventHandler? IsExpandedChanged;

    public BasePropertyEditorGroup(Type applicableType, GroupType groupType = GroupType.PrimaryExpander) : base(applicableType) {
        this.propObjs = new List<BasePropertyEditorObject>();
        this.PropertyObjects = this.propObjs.AsReadOnly();
        this.GroupType = groupType;
    }

    protected override void OnPropertyEditorChanged(PropertyEditor? oldEditor, PropertyEditor? newEditor) {
        base.OnPropertyEditorChanged(oldEditor, newEditor);
        foreach (BasePropertyEditorObject obj in this.propObjs) {
            SetPropertyEditor(obj, newEditor);
        }
    }

    public void ExpandHierarchy() {
        this.IsExpanded = true;
        foreach (BasePropertyEditorObject obj in this.propObjs) {
            if (obj is BasePropertyEditorGroup group) {
                group.ExpandHierarchy();
            }
        }
    }

    public void CollapseHierarchy() {
        // probably more performant to expand the top first, so that closing child ones won't cause rendering
        this.IsExpanded = false;
        foreach (BasePropertyEditorObject obj in this.propObjs) {
            if (obj is BasePropertyEditorGroup group) {
                group.CollapseHierarchy();
            }
        }
    }

    public virtual void AddItem(BasePropertyEditorObject propObj) => this.InsertItem(this.propObjs.Count, propObj);

    public virtual void InsertItem(int index, BasePropertyEditorObject propObj) {
        ArgumentNullException.ThrowIfNull(propObj);
        if (!this.IsPropertyEditorObjectAcceptable(propObj))
            throw new ArgumentException("The specific property editor object is not allowed: " + propObj);
        this.propObjs.Insert(index, propObj);
        OnAddedToGroup(propObj, this);
        this.ItemAdded?.Invoke(this, new ItemAddOrRemoveEventArgs<BasePropertyEditorObject>(index, propObj));
    }

    public virtual bool RemoveItem(BasePropertyEditorObject propObj) {
        int index = this.propObjs.IndexOf(propObj);
        if (index == -1)
            return false;
        this.RemoveItemAt(index);
        return true;
    }

    public virtual void RemoveItemAt(int index) {
        BasePropertyEditorObject propObj = this.propObjs[index];
        this.propObjs.RemoveAt(index);
        OnRemovedFromGroup(propObj, this);
        this.ItemRemoved?.Invoke(this, new ItemAddOrRemoveEventArgs<BasePropertyEditorObject>(index, propObj));
    }

    public virtual void MoveItem(int oldIndex, int newIndex) {
        BasePropertyEditorObject propObj = this.propObjs[oldIndex];
        this.propObjs.MoveItem(oldIndex, newIndex);
        this.ItemMoved?.Invoke(this, new ItemMoveEventArgs<BasePropertyEditorObject>(oldIndex, newIndex, propObj));
    }

    /// <summary>
    /// Used to determine if calling <see cref="InsertItem"/> or <see cref="AddItem"/> with the given object is allowed or not
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public abstract bool IsPropertyEditorObjectAcceptable(BasePropertyEditorObject obj);
}