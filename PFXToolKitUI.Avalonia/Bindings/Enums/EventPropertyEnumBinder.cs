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

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PFXToolKitUI.Avalonia.Bindings.Enums;

/// <summary>
/// A class which helps bind radio buttons to an enum property
/// </summary>
public class EventPropertyEnumBinder<TEnum> where TEnum : struct {
    private readonly Dictionary<RadioButton, TEnum> buttonToState; // all radio buttons
    private readonly Dictionary<TEnum, List<RadioButton>> stateToButtons; // maps enum to radio buttons
    private readonly Action<object, TEnum> setter;
    private readonly Func<object, TEnum> getter;
    private readonly AutoEventHelper eventHelper;
    private bool isAttaching, isUpdatingControls; // we track when updating controls to prevent possible stack overflow exception in weird setups.

    /// <summary>
    /// Gets or sets the active transferable data owner
    /// </summary>
    public object? Model { get; private set; }

    public EventPropertyEnumBinder(Type modelType, string eventName, Func<object, TEnum> getter, Action<object, TEnum> setter) {
        this.buttonToState = new Dictionary<RadioButton, TEnum>();
        this.stateToButtons = new Dictionary<TEnum, List<RadioButton>>();
        this.eventHelper = new AutoEventHelper(eventName, modelType, this.OnModelEnumChanged);
        this.setter = setter;
        this.getter = getter;
    }
    
    // Corruption can occur if the same button is associated with multiple enum values,
    // or vice verse. We do not check for such cases because that's your fault for not using the noggin :D

    public void Assign(RadioButton button, TEnum enumValue) {
        // If now added then add event, otherwise, just update target enum value
        if (this.buttonToState.TryAdd(button, enumValue)) {
            button.IsCheckedChanged += this.OnCheckChanged;
        }
        else {
            this.buttonToState[button] = enumValue;
        }
        
        if (!this.stateToButtons.TryGetValue(enumValue, out List<RadioButton>? list))
            this.stateToButtons[enumValue] = list = new List<RadioButton>();
        if (!list.Contains(button))
            list.Add(button);
    }

    public void Unassign(RadioButton button) {
        if (this.buttonToState.TryGetValue(button, out TEnum enumValue)) {
            if (this.stateToButtons.TryGetValue(enumValue, out List<RadioButton>? list)) {
                list.Remove(button);
            }

            button.IsCheckedChanged -= this.OnCheckChanged;
        }
    }

    public void UnassignAll() {
        foreach (KeyValuePair<RadioButton, TEnum> entry in this.buttonToState) {
            entry.Key.IsCheckedChanged -= this.OnCheckChanged;
        }

        this.buttonToState.Clear();
    }

    private void OnCheckChanged(object? sender, RoutedEventArgs e) {
        if (!this.isUpdatingControls && !this.isAttaching && this.Model != null) {
            RadioButton button = (RadioButton) sender!;
            if (button.IsChecked == true) {
                this.setter(this.Model, this.buttonToState[button]);
            }
        }
    }

    private void OnModelEnumChanged() {
        if (this.Model == null)
            throw new Exception("Fatal application bug");
        
        this.UpdateControls(this.getter(this.Model));
    }

    public void Attach(object model) {
        ArgumentNullException.ThrowIfNull(model);
        if (this.Model != null)
            throw new InvalidOperationException("Already attached");
        
        this.eventHelper.AddEventHandler(this.Model = model);
        this.UpdateControls(this.getter(model));
    }

    public void Detach() {
        if (this.Model == null)
            throw new InvalidOperationException("Not attached");

        this.eventHelper.RemoveEventHandler(this.Model);
        this.Model = null;
    }

    private void UpdateControls(TEnum value) {
        if (!this.stateToButtons.TryGetValue(value, out List<RadioButton>? list)) {
            return;
        }
        
        try {
            this.isUpdatingControls = true;
            List<RadioButton> setFalse = this.buttonToState.Keys.ToList();
            foreach (RadioButton button in list) {
                setFalse.Remove(button);
            }

            this.isAttaching = true;
            foreach (RadioButton button in setFalse)
                button.IsChecked = false;

            foreach (RadioButton button in list)
                button.IsChecked = true;
            this.isAttaching = false;
        }
        finally {
            this.isUpdatingControls = false;
        }
    }
}