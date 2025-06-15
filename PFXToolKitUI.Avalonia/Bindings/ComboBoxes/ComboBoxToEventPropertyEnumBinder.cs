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

using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PFXToolKitUI.PropertyEditing.DataTransfer.Enums;

namespace PFXToolKitUI.Avalonia.Bindings.ComboBoxes;

public class ComboBoxToEventPropertyEnumBinder<TEnum> where TEnum : struct, Enum {
    private static readonly ReadOnlyCollection<TEnum> ENUM_VALUES = DataParameterEnumInfo<TEnum>.EnumValues;

    private readonly Action<object, TEnum> setter;
    private readonly Func<object, TEnum?> getter;
    private readonly AutoEventHelper eventHelper;
    private bool isUpdatingControl;
    private DataParameterEnumInfo<TEnum>? enumInfo;

    /// <summary>
    /// Gets or sets the connected control
    /// </summary>
    public ComboBox? Control { get; private set; }

    /// <summary>
    /// Gets or sets the connected model
    /// </summary>
    public object? Model { get; private set; }

    public ComboBoxToEventPropertyEnumBinder(Type modelType, string eventName, Func<object, TEnum?> getter, Action<object, TEnum> setter) {
        this.eventHelper = new AutoEventHelper(eventName, modelType, this.OnModelEnumChanged);
        this.setter = setter;
        this.getter = getter;
    }

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
        this.eventHelper.AddEventHandler(model);

        this.Control!.Items.Clear();
        if ((info != null ? info.AllowedEnumList.Count : ENUM_VALUES.Count) > 0) {
            foreach (TEnum enumValue in info?.EnumList ?? ENUM_VALUES) {
                string displayText;
                if (info != null && info.EnumToText.TryGetValue(enumValue, out string? text)) {
                    displayText = text;
                }
                else {
                    displayText = enumValue.ToString();
                }

                this.Control.Items.Add(new ComboBoxItem() { Content = displayText, Tag = enumValue });
            }
        }

        this.UpdateControl(this.getter(model));
    }

    public void Detach() {
        if (this.Control == null)
            throw new InvalidOperationException("Not attached");

        this.Control.PropertyChanged -= this.OnControlPropertyChanged;
        this.eventHelper.RemoveEventHandler(this.Model!);
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