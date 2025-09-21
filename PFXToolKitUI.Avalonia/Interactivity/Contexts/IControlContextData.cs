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
using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.Avalonia.Interactivity.Contexts;

/// <summary>
/// An interface for mutable context data used to store context within a control.
/// This is a special implementation of <see cref="IContextData"/> that notifies
/// the data manager of changes, and supports batching multiple changes to avoid
/// excessive calls to <see cref="DataManager.InvalidateInheritedContext"/>
/// </summary>
public interface IControlContextData : IMutableContextData {
    /// <summary>
    /// Gets the control that owns this context data
    /// </summary>
    AvaloniaObject Owner { get; }

    /// <summary>
    /// Creates a new context data instance, which inherits data from the given context data. Inherited data is not prioritised 
    /// </summary>
    /// <param name="inherited">The data which is inherited</param>
    /// <returns>A new context data instance</returns>
    IControlContextData CreateInherited(IContextData inherited);
}