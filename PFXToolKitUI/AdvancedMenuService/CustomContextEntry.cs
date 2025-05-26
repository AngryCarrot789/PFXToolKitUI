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
using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.AdvancedMenuService;

/// <summary>
/// A context entry that has a <see cref="CanExecute"/> and <see cref="OnExecute"/> method
/// </summary>
public abstract class CustomContextEntry : BaseContextEntry {
    protected CustomContextEntry() {
    }

    protected CustomContextEntry(string displayName, string? description, Icon? icon = null) : base(displayName, description, icon) {
        
    }
    
    public virtual bool CanExecute(IContextData context) {
        return true;
    }

    public abstract Task OnExecute(IContextData context);
}