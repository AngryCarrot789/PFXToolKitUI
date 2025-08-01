﻿// 
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

namespace PFXToolKitUI.Utils;

public static class TaskUtils {
    /// <summary>
    /// Awaits the task. Returns null when it executes successfully. Returns the exception that the filter
    /// accepted, or throws when the filter does not accept the exception. 
    /// </summary>
    /// <param name="task"></param>
    /// <param name="filter"></param>
    /// <returns>The exception that was filtered</returns>
    public static async Task<Exception?> AwaitOrCatch(this Task task, Predicate<Exception> filter) {
        try {
            await task;
            return null;
        }
        catch (Exception e) when (filter(e)) {
            return e;
        }
    }
}