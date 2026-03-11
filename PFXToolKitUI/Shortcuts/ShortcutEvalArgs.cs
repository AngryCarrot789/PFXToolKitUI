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

using PFXToolKitUI.Shortcuts.Inputs;
using PFXToolKitUI.Shortcuts.Keymapping;

namespace PFXToolKitUI.Shortcuts;

public readonly struct ShortcutEvalArgs {
    public readonly IInputStroke stroke;
    public readonly List<KeyMapEntry> shortcuts;
    public readonly List<(InputStateEntry, bool)> inputStates;
    public readonly Predicate<KeyMapEntry>? filter;
    public readonly bool canProcessInputStates;
    public readonly bool canInherit;

    public ShortcutEvalArgs(IInputStroke stroke, List<KeyMapEntry> shortcuts, List<(InputStateEntry, bool)> inputStates, Predicate<KeyMapEntry>? filter, bool canProcessInputStates, bool canInherit) {
        this.stroke = stroke;
        this.shortcuts = shortcuts;
        this.inputStates = inputStates;
        this.filter = filter;
        this.canProcessInputStates = canProcessInputStates;
        this.canInherit = canInherit;
    }
}