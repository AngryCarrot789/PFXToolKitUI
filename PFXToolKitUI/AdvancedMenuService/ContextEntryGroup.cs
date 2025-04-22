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

using PFXToolKitUI.Icons;
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.AdvancedMenuService;

public class ContextEntryGroup : BaseContextEntry {
    public ObservableList<IContextObject> Items { get; }

    public ContextEntryGroup(string displayName, string? description = null, Icon? icon = null, StretchMode stretchMode = StretchMode.None) : base(displayName, description, icon, stretchMode) {
        this.Items = new ObservableList<IContextObject>();
    }
}