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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;

namespace PFXToolKitUI.Avalonia.Utils;

public static class VisualTreeUtils {
    public static AvaloniaObject? FindNearestInheritedPropertyDefinitionForLogical<T>(AvaloniaProperty<T> property, StyledElement? target) {
        for (StyledElement? next = target; next != null; next = next.Parent) {
            Optional<T> localValue = next.GetBaseValue(property);
            if (!localValue.HasValue)
                continue;

            return next;
        }

        return null;
    }

    public static T? FindLogicalParent<T>(AvaloniaObject? obj, bool includeSelf = true) where T : class {
        if (obj == null)
            return null;
        if (includeSelf && obj is T)
            return (T) (object) obj;

        do {
            obj = (obj as StyledElement)?.Parent;
            if (obj == null)
                return null;
            if (obj is T)
                return (T) (object) obj;
        } while (true);
    }

    public static T? GetLastLogicalParent<T>(AvaloniaObject? obj) where T : class {
        T? lastParent = null;
        for (T? parent = FindLogicalParent<T>(obj, false); parent != null; parent = FindLogicalParent<T>((AvaloniaObject) (object) parent, false))
            lastParent = parent;

        return lastParent;
    }

    public static AvaloniaObject? FilterNearestLogicalElement(StyledElement source, Predicate<StyledElement> filter, bool includeSource = false) {
        for (StyledElement? next = includeSource ? source : source.Parent; next != null; next = next.Parent) {
            if (filter(next)) {
                return next;
            }
        }

        return null;
    }

    public static InputElement? FindNearestLogicalFocusableElement(StyledElement source, bool includeSource = false) {
        return (InputElement?) FilterNearestLogicalElement(source, (o) => o is InputElement ie && ie.Focusable, includeSource);
    }

    public static bool TryMoveFocusUpwards(InputElement source) {
        InputElement? nearest = FindNearestLogicalFocusableElement(source, false);
        if (nearest != null && nearest.Focus()) {
            return true;
        }
        else if (TopLevel.GetTopLevel(source) is TopLevel topLevel) {
            if (source != topLevel && topLevel.Focus()) {
                return true;
            }
        }

        return false;
    }
}