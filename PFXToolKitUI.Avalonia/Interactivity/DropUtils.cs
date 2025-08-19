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

using Avalonia.Input;
using PFXToolKitUI.Interactivity;

namespace PFXToolKitUI.Avalonia.Interactivity;

public static class DropUtils {
    private const KeyModifiers ControlShift = KeyModifiers.Control | KeyModifiers.Shift;

    /// <summary>
    /// Gets the final intended drop type based on the allowed drop types and the modifier keys pressed
    /// </summary>
    /// <param name="modifiers">The pressed modifiers</param>
    /// <param name="allowedDropTypes">The allowed drop types</param>
    /// <returns>
    /// A single drop type based on the modifiers pressed. When no modifiers are pressed,
    /// the default order is to try and move first, then copy, then link.
    /// </returns>
    public static EnumDropType GetDropFromPressedModifiers(KeyModifiers modifiers, EnumDropType allowedDropTypes) {
        // keyStates &= ~MouseButtons; // remove mouse buttons
        if ((modifiers & ControlShift) == ControlShift && (allowedDropTypes & EnumDropType.Link) != 0) {
            return EnumDropType.Link; // Hold CTRL + SHIFT to create link
        }
        else if ((modifiers & KeyModifiers.Alt) == KeyModifiers.Alt && (allowedDropTypes & EnumDropType.Link) != 0) {
            return EnumDropType.Link; // Hold ALT to create link
        }
        else if ((modifiers & KeyModifiers.Shift) == KeyModifiers.Shift && (allowedDropTypes & EnumDropType.Move) != 0) {
            return EnumDropType.Move; // Hold SHIFT to move.
        }
        else if ((modifiers & KeyModifiers.Control) == KeyModifiers.Control && (allowedDropTypes & EnumDropType.Copy) != 0) {
            return EnumDropType.Copy; // Hold CTRL to top
        }
        else {
            // No modifiers press. So by default we will try to move first, then try copy, then finally try link.
            if ((allowedDropTypes & EnumDropType.Move) != 0) {
                return EnumDropType.Move; // Try to move by default
            }
            else if ((allowedDropTypes & EnumDropType.Copy) != 0) {
                return EnumDropType.Copy; // Try to copy by default
            }
            else if ((allowedDropTypes & EnumDropType.Link) != 0) {
                return EnumDropType.Link; // Try to link by default
            }
            else {
                return EnumDropType.None; // None of the above will work so no drag drop for you :)
            }
        }
    }
}