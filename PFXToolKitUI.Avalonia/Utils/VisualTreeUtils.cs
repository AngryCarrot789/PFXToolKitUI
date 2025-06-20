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

using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;

namespace PFXToolKitUI.Avalonia.Utils;

public static class VisualTreeUtils {
    public static AvaloniaObject? FindNearestInheritedPropertyDefinition<T>(AvaloniaProperty<T> property, StyledElement? target) {
        for (StyledElement? next = target; next != null; next = GetParent(next)) {
            Optional<T> localValue = next.GetBaseValue(property);
            if (!localValue.HasValue)
                continue;

            return next;
        }

        return null;
    }

    /// <summary>
    /// Gets the parent of the given source object
    /// </summary>
    /// <param name="source">The object to get the parent of</param>
    /// <param name="visualOnly">
    /// True to only allow the visual parent, otherwise false to allow visual,
    /// logical and templated parents (in that order based on availability)
    /// </param>
    /// <returns>The parent, or null, if there was no parent available</returns>
    public static AvaloniaObject? GetParent(AvaloniaObject? source, bool visualOnly = false) => GetParent(source as StyledElement, visualOnly);

    public static StyledElement? GetParent(StyledElement? source, bool visualOnly = false) {
        if (source == null)
            return null;

        StyledElement? parent = source.Parent;
        if (parent == null && !visualOnly && source is TemplatedControl control) {
            return control.Parent ?? control.TemplatedParent as StyledElement;
        }

        return parent;
    }

    /// <summary>
    /// Gets the first parent object of the given type
    /// </summary>
    /// <param name="obj">A child object</param>
    /// <param name="includeSelf">True to check if obj is of the given generic type, and if so, return that. Otherwise, scan parents</param>
    /// <typeparam name="T">Type of parent</typeparam>
    /// <returns>The parent, or null, if none of the given type were found</returns>
    public static T? GetParent<T>(AvaloniaObject? obj, bool includeSelf = true, bool visualOnly = false) where T : class {
        if (obj == null)
            return null;
        if (includeSelf && obj is T)
            return (T) (object) obj;

        do {
            obj = GetParent(obj, visualOnly);
            if (obj == null)
                return null;
            if (obj is T)
                return (T) (object) obj;
        } while (true);
    }

    public static T? GetLastParent<T>(AvaloniaObject? obj, bool visualOnly = false) where T : class {
        T? lastParent = null;
        for (T? parent = GetParent<T>(obj, false, visualOnly); parent != null; parent = GetParent<T>((AvaloniaObject) (object) parent, false, visualOnly))
            lastParent = parent;

        return lastParent;
    }

    public static bool TryGetParent<T>(AvaloniaObject? obj, [NotNullWhen(true)] out T? theParent, bool includeSelf = true, bool visualOnly = false) where T : class {
        return (theParent = GetParent<T>(obj, includeSelf, visualOnly)) != null;
    }

    public static bool TryGetLastParent<T>(AvaloniaObject? obj, [NotNullWhen(true)] out T? theParent, bool visualOnly = false) where T : class {
        return (theParent = GetLastParent<T>(obj, visualOnly)) != null;
    }

    /// <summary>
    /// Calculates if the given item is either the given templated item type, or it is a templated child of the type.
    /// <para>
    /// This is the same as invoking <see cref="GetParent{T}"/> with self included and checking the value is non-null
    /// </para>
    /// </summary>
    public static bool IsTemplatedItemOrDescended<T>(AvaloniaObject? obj) where T : class {
        return GetParent<T>(obj, true, false) != null;
    }

    public static AvaloniaObject? FilterFindNearestElement(AvaloniaObject source, Predicate<AvaloniaObject> filter, bool includeSource = false) {
        for (AvaloniaObject? next = includeSource ? source : GetParent(source); next != null; next = GetParent(next)) {
            if (filter(next)) {
                return next;
            }
        }

        return null;
    }

    public static InputElement? FindNearestFocusableElement(AvaloniaObject source, bool includeSource = false) {
        return (InputElement?) FilterFindNearestElement(source, (o) => o is InputElement ie && ie.Focusable, includeSource);
    }

    public static bool TryMoveFocusUpwards(InputElement source) {
        InputElement? nearest = FindNearestFocusableElement(source, false);
        if (nearest != null && nearest.Focus()) {
            return true;
        }
        else if (TopLevel.GetTopLevel(source) is TopLevel topLevel) {
            // It's obsolete but it's the only thing we can try
            topLevel.FocusManager?.ClearFocus();
            return true;
        }

        return false;
    }
}