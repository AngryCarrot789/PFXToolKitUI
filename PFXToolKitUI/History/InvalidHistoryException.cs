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

namespace PFXToolKitUI.History;

/// <summary>
/// An exception thrown when the state of the application was invalid and a history action could not be undone
/// </summary>
public class InvalidHistoryException : InvalidOperationException {
    public InvalidHistoryException(string? message) : base(message) {
    }

    public InvalidHistoryException(string? message, Exception? innerException) : base(message, innerException) {
    }

    public static void Assert(bool condition, string message) {
        if (!condition) {
            throw new InvalidHistoryException(message);
        }
    }
}