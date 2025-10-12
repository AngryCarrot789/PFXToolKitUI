// 
// Copyright (c) 2023-2025 REghZy
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

namespace PFXToolKitUI.AdvancedMenuService;

/// <summary>
/// An interface for any "group" within a menu system.
/// <para>
/// Fixed groups, represented by <see cref="FixedWeightedMenuEntryGroup"/>, have fixed number of
/// <see cref="IMenuEntry"/> entries typically created before the context registry is fully initialised.
/// Dynamically changing visibility, caption, tooltip, icons, etc. is possible, but is tedious.
/// </para>
/// <para>
/// Dynamic groups, represented by <see cref="DynamicWeightedMenuEntryGroup"/>, generate their entries
/// when required. Their generator callback is given the context available at generation, which
/// allows for highly customisable menu options based on the available context and therefore states of
/// objects.
/// </para>
/// </summary>
public interface IWeightedMenuEntryGroup;