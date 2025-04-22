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

using System.Windows.Input;

namespace PFXToolKitUI.Utils.Commands;

/// <summary>
/// An interface for general relay commands
/// </summary>
public interface IRelayCommand : ICommand {
    /// <summary>
    /// Whether this relay command is enabled and can be executed. Affects the result of <see cref="ICommand.CanExecute"/>
    /// </summary>
    bool IsEnabled { get; set; }
}