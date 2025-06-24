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

namespace PFXToolKitUI.Utils;

/// <summary>
/// A mode for how to treat a decimal number whose decimal part is non-zero
/// </summary>
public enum RoundingMode {
    /// <summary>
    /// Does nothing. This may not be valid in all cases, meaning this value may default to <see cref="Truncate"/>
    /// </summary>
    None,

    /// <summary>
    /// Casts the decimal number to an integer number. May not result in the desired effect with negative numbers
    /// </summary>
    Truncate,

    /// <summary>
    /// Floors the decimal number then casts to an integer number
    /// </summary>
    Floor,

    /// <summary>
    /// Ceilings the decimal number then casts to an integer number
    /// </summary>
    Ceil,

    /// <summary>
    /// Rounds the decimal number to the nearest integer number
    /// </summary>
    Round
}