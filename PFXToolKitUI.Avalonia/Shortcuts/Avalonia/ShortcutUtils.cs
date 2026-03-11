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
using PFXToolKitUI.Shortcuts.Inputs;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Shortcuts.Avalonia;

public static class ShortcutUtils {
    public static bool GetKeyStrokeForEvent(KeyEventArgs e, out KeyStroke stroke, bool isRelease) {
        // Key key = e.Key == Key.System ? (Key) e.PhysicalKey : e.Key;
        if (IsModifierKey(e.Key) || e.Key == Key.DeadCharProcessed) {
            stroke = default;
            return false;
        }

        stroke = new KeyStroke((int) e.Key, (int) e.KeyModifiers, isRelease);
        return true;
    }

    public static bool IsModifierKey(Key key) {
        switch (key) {
            case Key.LeftCtrl:
            case Key.RightCtrl:
            case Key.LeftAlt:
            case Key.RightAlt:
            case Key.LeftShift:
            case Key.RightShift:
            case Key.LWin:
            case Key.RWin:
            case Key.Clear:
            case Key.OemClear:
            case Key.Apps:
                return true;
            default: return false;
        }
    }

    public static MouseStroke GetMouseStrokeForEvent(PointerPressedEventArgs e) {
        int modifiers = (int) e.KeyModifiers;
        int button;
        if (e.Properties.IsLeftButtonPressed)
            button = (int) MouseButton.Left;
        else if (e.Properties.IsMiddleButtonPressed)
            button = (int) MouseButton.Middle;
        else if (e.Properties.IsRightButtonPressed)
            button = (int) MouseButton.Right;
        else if (e.Properties.IsXButton1Pressed)
            button = (int) MouseButton.XButton1;
        else if (e.Properties.IsXButton2Pressed)
            button = (int) MouseButton.XButton2;
        else
            button = 0;

        return new MouseStroke(button, modifiers, false);
    }

    public static bool GetMouseStrokeForEvent(PointerWheelEventArgs e, out MouseStroke stroke) {
        // TODO
        double delta = e.Delta.X;
        if (DoubleUtils.AreClose(delta, 0.0) && DoubleUtils.AreClose(delta = e.Delta.Y, 0.0)) {
            stroke = default;
            return false;
        }

        stroke = new MouseStroke(
            delta > 0 ? AvaloniaKeyMapManager.BUTTON_WHEEL_UP : AvaloniaKeyMapManager.BUTTON_WHEEL_DOWN,
            (int) e.KeyModifiers, false, wheelDelta: (int) delta);
        return true;
    }
}