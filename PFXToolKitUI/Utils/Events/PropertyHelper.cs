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

namespace PFXToolKitUI.Utils.Events;

/// <summary>
/// A helper class for managing properties
/// </summary>
public static class PropertyHelper {
    #region Ignore equal

    public static void SetAndRaise<T>(ref T field, T newValue, Action onValueChanged) {
        field = newValue;
        onValueChanged();
    }

    public static void SetAndRaise<T, TInstance>(ref T field, T newValue, TInstance instance, Action<TInstance> onValueChanged) {
        field = newValue;
        onValueChanged(instance);
    }

    public static void SetAndRaise<T, TInstance>(ref T field, T newValue, TInstance instance, Action<TInstance, T, T> onValueChanged) {
        T oldValue = field;
        field = newValue;
        onValueChanged(instance, oldValue, newValue);
    }

    public static void SetAndRaise<T>(ref T field, T newValue, object sender, EventHandler? onValueChanged) {
        field = newValue;
        onValueChanged?.Invoke(sender, EventArgs.Empty);
    }

    public static void SetAndRaise<T>(ref T field, T newValue, object sender, EventHandler<ValueChangedEventArgs<T>>? onValueChanged) {
        T oldValue = field;
        field = newValue;
        onValueChanged?.Invoke(sender, new ValueChangedEventArgs<T>(oldValue, newValue));
    }

    public static void SetAndRaise<T>(ref T field, T newValue, object sender, EventHandler<T>? onValueChanged) {
        field = newValue;
        onValueChanged?.Invoke(sender, newValue);
    }

    #endregion

    #region If-not-equal

    public static void SetAndRaiseINE<T>(ref T field, T newValue, Action onValueChanged) {
        if (!EqualityComparer<T>.Default.Equals(field, newValue)) {
            field = newValue;
            onValueChanged();
        }
    }

    /// <summary>
    /// Checks that <see cref="field"/> does not equal <see cref="newValue"/> and only then sets <see cref="field"/>
    /// to <see cref="newValue"/> and then invokes <see cref="onValueChanged"/> passing in the <see cref="instance"/> parameter 
    /// </summary>
    /// <param name="field">The field to set when it does not equal the new value</param>
    /// <param name="newValue">The new value</param>
    /// <param name="instance">A parameter to pass to <see cref="onValueChanged"/></param>
    /// <param name="onValueChanged">A callback to fire when the field differs from the new value. Invoked after updating the field</param>
    /// <typeparam name="T">The type of value</typeparam>
    /// <typeparam name="TInstance">The type of parameter</typeparam>
    public static void SetAndRaiseINE<T, TInstance>(ref T field, T newValue, TInstance instance, Action<TInstance> onValueChanged) {
        if (!EqualityComparer<T>.Default.Equals(field, newValue)) {
            field = newValue;
            onValueChanged(instance);
        }
    }

    /// <summary>
    /// Checks that <see cref="field"/> does not equal <see cref="newValue"/> and only then sets <see cref="field"/>
    /// to <see cref="newValue"/> and then invokes <see cref="onValueChanged"/> passing in the <see cref="instance"/> parameter 
    /// </summary>
    /// <param name="field">The field to set when it does not equal the new value</param>
    /// <param name="newValue">The new value</param>
    /// <param name="equals">
    /// A function that returns true when the two values are equal. When it returns 
    /// false, we can update the field and invoke the callback
    /// </param>
    /// <param name="instance">A parameter to pass to <see cref="onValueChanged"/></param>
    /// <param name="onValueChanged">A callback to fire when the field differs from the new value. Invoked after updating the field</param>
    /// <typeparam name="T">The type of value</typeparam>
    /// <typeparam name="TInstance">The type of parameter</typeparam>
    public static void SetAndRaiseINE<T, TInstance>(ref T field, T newValue, Func<T, T, bool> equals, TInstance instance, Action<TInstance> onValueChanged) {
        if (!equals(field, newValue)) {
            field = newValue;
            onValueChanged(instance);
        }
    }

    /// <summary>
    /// Checks that <see cref="field"/> does not equal <see cref="newValue"/> and only then sets <see cref="field"/>
    /// to <see cref="newValue"/> and then invokes <see cref="onValueChanged"/> passing in the <see cref="instance"/>
    /// parameter and the old and new values
    /// </summary>
    /// <param name="field">The field to set when it does not equal the new value</param>
    /// <param name="newValue">The new value</param>
    /// <param name="instance">A parameter to pass to <see cref="onValueChanged"/></param>
    /// <param name="onValueChanged">A callback to fire when the field differs from the new value. Invoked after updating the field</param>
    /// <typeparam name="T">The type of value</typeparam>
    /// <typeparam name="TInstance">The type of parameter</typeparam>
    public static void SetAndRaiseINE<T, TInstance>(ref T field, T newValue, TInstance instance, Action<TInstance, T, T> onValueChanged) {
        T oldValue = field;
        if (!EqualityComparer<T>.Default.Equals(oldValue, newValue)) {
            field = newValue;
            onValueChanged(instance, oldValue, newValue);
        }
    }

    /// <summary>
    /// Checks that <see cref="field"/> does not equal <see cref="newValue"/> and only then sets <see cref="field"/>
    /// to <see cref="newValue"/> and then invokes <see cref="onValueChanged"/> passing in the <see cref="instance"/>
    /// parameter and the old and new values
    /// </summary>
    /// <param name="field">The field to set when it does not equal the new value</param>
    /// <param name="newValue">The new value</param>
    /// <param name="equals">
    /// A function that returns true when the two values are equal. When it returns 
    /// false, we can update the field and invoke the callback
    /// </param>
    /// <param name="instance">A parameter to pass to <see cref="onValueChanged"/></param>
    /// <param name="onValueChanged">A callback to fire when the field differs from the new value. Invoked after updating the field</param>
    /// <typeparam name="T">The type of value</typeparam>
    /// <typeparam name="TInstance">The type of parameter</typeparam>
    public static void SetAndRaiseINE<T, TInstance>(ref T field, T newValue, Func<T, T, bool> equals, TInstance instance, Action<TInstance, T, T> onValueChanged) {
        T oldValue = field;
        if (!equals(oldValue, newValue)) {
            field = newValue;
            onValueChanged(instance, oldValue, newValue);
        }
    }

    /// <summary>
    /// Checks that the field does not equal the new value, and updates the field and invokes <see cref="onValueChanged"/> (if non-null) with the given instance
    /// </summary>
    /// <param name="field">The field to set when it does not equal the new value</param>
    /// <param name="newValue">The new value</param>
    /// <param name="sender">The parameter to pass to the event handler</param>
    /// <param name="onValueChanged">The callback to fire when the field differs from the new value. Invoked after updating the field</param>
    /// <typeparam name="T">The type of value</typeparam>
    public static void SetAndRaiseINE<T>(ref T field, T newValue, object sender, EventHandler? onValueChanged) {
        if (!EqualityComparer<T>.Default.Equals(field, newValue)) {
            field = newValue;
            onValueChanged?.Invoke(sender, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Checks that the field does not equal the new value, and updates the field and invokes <see cref="onValueChanged"/> (if non-null) with the given instance.
    /// </summary>
    /// <param name="field">The field to set when it does not equal the new value</param>
    /// <param name="newValue">The new value</param>
    /// <param name="sender">The parameter to pass to the event handler</param>
    /// <param name="onValueChanged">The callback to fire when the field differs from the new value. Invoked after updating the field</param>
    /// <typeparam name="T">The type of value that changed</typeparam>
    public static void SetAndRaiseINE<T>(ref T field, T newValue, object sender, EventHandler<ValueChangedEventArgs<T>>? onValueChanged) {
        T oldValue = field;
        if (!EqualityComparer<T>.Default.Equals(oldValue, newValue)) {
            field = newValue;
            onValueChanged?.Invoke(sender, new ValueChangedEventArgs<T>(oldValue, newValue));
        }
    }

    /// <summary>
    /// Checks that the field does not equal the new value, and updates the field and invokes <see cref="onValueChanged"/> (if non-null) with the given instance.
    /// </summary>
    /// <param name="field">The field to set when it does not equal the new value</param>
    /// <param name="newValue">The new value</param>
    /// <param name="sender">The parameter to pass to the event handler</param>
    /// <param name="onValueChanged">The callback to fire when the field differs from the new value. Invoked after updating the field</param>
    /// <typeparam name="T">The type of value that changed</typeparam>
    public static void SetAndRaiseINE<T>(ref T field, T newValue, object sender, EventHandler<T>? onValueChanged) {
        if (!EqualityComparer<T>.Default.Equals(field, newValue)) {
            field = newValue;
            onValueChanged?.Invoke(sender, newValue);
        }
    }

    public static void SetAndRaiseINE<T>(ref T field, T newValue, Func<T, T, bool> equals, object sender, EventHandler? onValueChanged) {
        if (!equals(field, newValue)) {
            field = newValue;
            onValueChanged?.Invoke(sender, EventArgs.Empty);
        }
    }

    public static void SetAndRaiseINE<T>(ref T field, T newValue, Func<T, T, bool> equals, object sender, EventHandler<ValueChangedEventArgs<T>>? onValueChanged) {
        T oldValue = field;
        if (!equals(oldValue, newValue)) {
            field = newValue;
            onValueChanged?.Invoke(sender, new ValueChangedEventArgs<T>(oldValue, newValue));
        }
    }

    public static void SetAndRaiseINE<T>(ref T field, T newValue, Func<T, T, bool> equals, object sender, EventHandler<T>? onValueChanged) {
        if (!equals(field, newValue)) {
            field = newValue;
            onValueChanged?.Invoke(sender, newValue);
        }
    }

    public static void SetAndRaiseINEEx<TOwner, T>(TOwner propertyOwner, Func<TOwner, T> getter, Action<TOwner, T> setter, T newValue, Action onValueChanged) {
        if (!EqualityComparer<T>.Default.Equals(getter(propertyOwner), newValue)) {
            setter(propertyOwner, newValue);
            onValueChanged();
        }
    }

    public static void SetAndRaiseINEEx<TOwner, T, TInstance>(TOwner propertyOwner, Func<TOwner, T> getter, Action<TOwner, T> setter, T newValue, TInstance instance, Action<TInstance> onValueChanged) {
        T oldValue = getter(propertyOwner);
        if (!EqualityComparer<T>.Default.Equals(oldValue, newValue)) {
            setter(propertyOwner, newValue);
            onValueChanged(instance);
        }
    }

    public static void SetAndRaiseINEEx<TOwner, T, TInstance>(TOwner propertyOwner, Func<TOwner, T> getter, Action<TOwner, T> setter, T newValue, TInstance instance, Action<TInstance, T, T> onValueChanged) {
        T oldValue = getter(propertyOwner);
        if (!EqualityComparer<T>.Default.Equals(oldValue, newValue)) {
            setter(propertyOwner, newValue);
            onValueChanged(instance, oldValue, newValue);
        }
    }

    public static void SetAndRaiseINEEx<TOwner, T>(TOwner propertyOwner, Func<TOwner, T> getter, Action<TOwner, T> setter, T newValue, object sender, EventHandler? onValueChanged) {
        T oldValue = getter(propertyOwner);
        if (!EqualityComparer<T>.Default.Equals(oldValue, newValue)) {
            setter(propertyOwner, newValue);
            onValueChanged?.Invoke(sender, EventArgs.Empty);
        }
    }

    public static void SetAndRaiseINEEx<TOwner, T>(TOwner propertyOwner, Func<TOwner, T> getter, Action<TOwner, T> setter, T newValue, object instance, EventHandler<ValueChangedEventArgs<T>>? onValueChanged) {
        T oldValue = getter(propertyOwner);
        if (!EqualityComparer<T>.Default.Equals(oldValue, newValue)) {
            setter(propertyOwner, newValue);
            onValueChanged?.Invoke(instance, new ValueChangedEventArgs<T>(oldValue, newValue));
        }
    }

    public static void SetAndRaiseINEEx<TOwner, T>(TOwner propertyOwner, Func<TOwner, T> getter, Action<TOwner, T> setter, T newValue, object instance, EventHandler<T>? onValueChanged) {
        T oldValue = getter(propertyOwner);
        if (!EqualityComparer<T>.Default.Equals(oldValue, newValue)) {
            setter(propertyOwner, newValue);
            onValueChanged?.Invoke(instance, newValue);
        }
    }

    #endregion
}