// 
// Copyright (c) 2024-2025 REghZy
// 
// This file is part of FramePFX.
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
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

namespace PFXToolKitUI.Avalonia.Services.Windowing;

public interface IWindowContent {
    /// <summary>
    /// Invoked when the window opens
    /// </summary>
    /// <param name="window"></param>
    void OnOpened(IWindow window);
    
    /// <summary>
    /// Invoked when the window closes
    /// </summary>
    /// <param name="window"></param>
    void OnClosed(IWindow window);
}