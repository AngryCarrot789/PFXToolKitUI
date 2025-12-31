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

using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PFXToolKitUI.EventHelpers;
using PFXToolKitUI.PropertyEditing.DataTransfer.Enums;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Bindings.ComboBoxes;

public class ComboBoxToEventPropertyEnumBinder<TEnum> : IRelayEventHandler where TEnum : unmanaged, Enum {
    private static readonly ReadOnlyCollection<TEnum> ENUM_VALUES = EnumInfo<TEnum>.EnumValues;

    private readonly Action<object, TEnum> setter;
    private readonly Func<object, TEnum?> getter;
    private readonly EventWrapper eventRelay;
    private bool isUpdatingControl;
    private DataParameterEnumInfo<TEnum>? enumInfo;
    
    private readonly Dictionary<TEnum, ComboBoxItem> enumToItem = new Dictionary<TEnum, ComboBoxItem>();
    private readonly Dictionary<TEnum, bool> enumToEnabledState = new Dictionary<TEnum, bool>();

    /// <summary>
    /// Gets or sets the connected control
    /// </summary>
    public ComboBox? Control { get; private set; }

    /// <summary>
    /// Gets or sets the connected model
    /// </summary>
    public object? Model { get; private set; }

    /// <summary>
    /// Gets or sets the complement value compared with the enabled state in our internal dictionary
    /// to produce the final enabled state of items. Default value is true
    /// </summary>
    public bool EnabledComplement {
        get => field;
        set {
            if (field != value) {
                field = value;
                this.UpdateIsEnabledForAll();
            }
        }
    } = true;

    public ComboBoxToEventPropertyEnumBinder(Type modelType, string eventName, Func<object, TEnum?> getter, Action<object, TEnum> setter) {
        this.eventRelay = EventRelayStorage.UIStorage.GetEventRelay(modelType, eventName);
        this.setter = setter;
        this.getter = getter;
    }

    void IRelayEventHandler.OnEvent(object sender) => this.OnModelEnumChanged();

    private void OnModelEnumChanged() {
        if (this.Model == null)
            throw new Exception("Fatal application bug");

        this.UpdateControl(this.getter(this.Model!));
    }

    private void OnControlPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
        if (!this.isUpdatingControl && e.Property == SelectingItemsControl.SelectedIndexProperty) {
            int idx = ((AvaloniaPropertyChangedEventArgs<int>) e).NewValue.GetValueOrDefault();
            this.setter(this.Model!, idx == -1 ? default : (this.enumInfo?.EnumList ?? ENUM_VALUES)[idx]);
        }
    }

    public void Attach(ComboBox comboBox, object model, DataParameterEnumInfo<TEnum>? info = null) {
        ArgumentNullException.ThrowIfNull(comboBox);
        ArgumentNullException.ThrowIfNull(model);
        if (this.Control != null)
            throw new InvalidOperationException("Already attached");

        this.Control = comboBox;
        this.Model = model;
        this.enumInfo = info;
        this.Control.PropertyChanged += this.OnControlPropertyChanged;
        EventRelayStorage.UIStorage.AddHandler(this.Model!, this, this.eventRelay);

        this.isUpdatingControl = true;
        this.Control!.Items.Clear();
        this.enumToItem.Clear();
        
        if ((info != null ? info.AllowedEnumList.Count : ENUM_VALUES.Count) > 0) {
            foreach (TEnum value in info?.EnumList ?? ENUM_VALUES) {
                string displayText;
                if (info != null && info.EnumToText.TryGetValue(value, out string? text)) {
                    displayText = text;
                }
                else {
                    displayText = value.ToString();
                }

                ComboBoxItem cbi = new ComboBoxItem() { Content = displayText, Tag = value };
                this.UpdateIsEnabled(cbi, value);
                this.enumToItem[value] = cbi;
                this.Control.Items.Add(cbi);
            }
        }

        this.isUpdatingControl = false;
        this.UpdateControl(this.getter(model));
    }

    private bool GetIsEnumEnabled(TEnum value) {
        return !this.enumToEnabledState.TryGetValue(value, out bool b) || b;
    }

    private void UpdateIsEnabled(ComboBoxItem item) => this.UpdateIsEnabled(item, (TEnum) item.Tag!);
    
    private void UpdateIsEnabled(ComboBoxItem item, TEnum value) {
        item.IsEnabled = this.EnabledComplement == this.GetIsEnumEnabled(value);
    }
    
    private void UpdateIsEnabled(TEnum value) {
        if (this.enumToItem.TryGetValue(value, out ComboBoxItem? comboBoxItem)) {
            this.UpdateIsEnabled(comboBoxItem, value);
        }
    }

    private void UpdateIsEnabledForAll() {
        if (this.Control != null) {
            foreach (object? item in this.Control.Items) {
                this.UpdateIsEnabled((ComboBoxItem) item!);
            }
        }
    }

    public void SetIsEnabled(TEnum value, bool? state) {
        if (state.HasValue) {
            if (!this.enumToEnabledState.TryGetValue(value, out bool b) || b != state.Value) {
                this.enumToEnabledState[value] = state.Value;
                this.UpdateIsEnabled(value);
            }
        }
        else if (this.enumToEnabledState.Remove(value)) {
            this.UpdateIsEnabled(value);
        }
    }

    public void Detach() {
        if (this.Control == null)
            throw new InvalidOperationException("Not attached");

        this.Control.PropertyChanged -= this.OnControlPropertyChanged;
        EventRelayStorage.UIStorage.RemoveHandler(this.Model!, this, this.eventRelay);
        this.Control = null;
        this.Model = null;
        this.enumInfo = null;
    }

    private void UpdateControl(TEnum? currentValue) {
        try {
            this.isUpdatingControl = true;
            if (currentValue.HasValue) {
                this.Control!.SelectedIndex = (this.enumInfo?.EnumList ?? ENUM_VALUES).IndexOf(currentValue.Value);
            }
            else {
                this.Control!.SelectedIndex = -1;
            }
        }
        finally {
            this.isUpdatingControl = false;
        }
    }
}