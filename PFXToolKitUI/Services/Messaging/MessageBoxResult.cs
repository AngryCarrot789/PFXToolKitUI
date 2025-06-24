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

namespace PFXToolKitUI.Services.Messaging;

/// <summary>
/// An enum for the result of a simple message box
/// </summary>
public enum MessageBoxResult {
    /// <summary>
    /// Nothing was clicked (user force closed the window, e.g. by clicking the close button or pressing Esc)
    /// </summary>
    None = 0,

    /// <summary>
    /// User clicked OK
    /// </summary>
    OK = 1,

    /// <summary>
    /// User clicked Cancel
    /// </summary>
    Cancel = 2,

    /// <summary>
    /// User clicked Yes
    /// </summary>
    Yes = 3,

    /// <summary>
    /// User clicked No
    /// </summary>
    No = 4,
}