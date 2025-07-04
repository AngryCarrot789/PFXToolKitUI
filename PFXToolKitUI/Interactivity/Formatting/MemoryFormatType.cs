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

namespace PFXToolKitUI.Interactivity.Formatting;

public enum MemoryFormatType {
    /// <summary>
    /// A single bit
    /// </summary>
    Bit,

    /// <summary>
    /// 8 bits
    /// </summary>
    Byte,

    // Here we go, IEC kicking us in the butt cracks to make life harder

    KiloBit,
    KiloByte1000,
    KibiByte1024,
    MegaBit,
    MegaByte1000,
    MebiByte1024,
    GigaBit,
    GigaByte1000,
    GibiByte1024,
    TeraBit,
    TeraByte1000,
    TebiByte1024
}