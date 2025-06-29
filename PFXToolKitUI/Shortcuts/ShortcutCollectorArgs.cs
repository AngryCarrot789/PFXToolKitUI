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

namespace PFXToolKitUI.Shortcuts;

public readonly struct ShortcutCollectorArgs {
    public readonly IInputStroke stroke;
    public readonly List<ShortcutEntry> list;
    public readonly Predicate<ShortcutEntry> filter;

    public ShortcutCollectorArgs(IInputStroke stroke, List<ShortcutEntry> list, Predicate<ShortcutEntry> filter) {
        this.stroke = stroke;
        this.list = list;
        this.filter = filter;
    }
}