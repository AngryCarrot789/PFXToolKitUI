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

namespace PFXToolKitUI.Avalonia.Interactivity.Contexts;

/// <summary>
/// Context data for a control that automatically invalidates the control's inherited context data when modifying this instance
/// </summary>
public sealed class ControlContextData(AvaloniaObject owner) : BaseControlContextData(owner) {
    public ControlContextData(AvaloniaObject owner, InheritingControlContextData? copyFromNonInherited) : this(owner) {
        this.CopyFrom(copyFromNonInherited?.NonInheritedEntries);
    }
}