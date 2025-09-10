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

namespace PFXToolKitUI.Services.UserInputs;

public interface IUserInputDialogService {
    public static IUserInputDialogService Instance => ApplicationPFX.GetService<IUserInputDialogService>();

    /// <summary>
    /// Shows an input dialog using the given information object
    /// </summary>
    /// <param name="info">The information to present in the dialog</param>
    /// <returns>
    /// An async boolean. True when closed successfully (you can accept the results, and trust the
    /// validation function was run), False when validation fails or the text field is empty and
    /// empty is disabled, or Null when the dialog closed unexpectedly</returns>
    Task<bool?> ShowInputDialogAsync(UserInputInfo info);
}