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

using Avalonia;
using PFXToolKitUI.Services.UserInputs;

namespace PFXToolKitUI.Avalonia.Services.UserInputs;

/// <summary>
/// An interface for user input content controls
/// </summary>
public interface IUserInputContent {
    void Connect(UserInputDialogView dialog, UserInputInfo info);

    void Disconnect();

    /// <summary>
    /// Try to focus the primary input field. If nothing exists to focus, this
    /// returns false which usually results in the confirm or cancel button being focused
    /// </summary>
    /// <returns></returns>
    bool FocusPrimaryInput();

    /// <summary>
    /// Fired when the window opens. Is not called when we become connected to a UserInputDialogView whose window is already open.
    /// </summary>
    void OnWindowOpening() {
    }

    /// <summary>
    /// Fired when the window opens. Is not called when we become connected to a UserInputDialogView whose window is already open.
    /// </summary>
    void OnWindowOpened() {
    }

    /// <summary>
    /// Fired when the window closes. Is not called when we become disconnected from a UserInputDialogView whose window is open.
    /// </summary>
    void OnWindowClosed() {
    }

    PixelSize GetMinimumBounds() => PixelSize.Empty;
}