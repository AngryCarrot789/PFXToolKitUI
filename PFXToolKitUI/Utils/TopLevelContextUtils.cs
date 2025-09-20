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

using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Interactivity.Windowing;

namespace PFXToolKitUI.Utils;

public static class TopLevelContextUtils {
    /// <summary>
    /// Tries to get a useful top level from the current command context as well as the optional alternate context
    /// </summary>
    public static ITopLevel? GetUsefulTopLevel(IContextData? alternateContext = null) {
        ITopLevel? parentWindow = null;
        if (CommandManager.LocalContextManager.TryGetGlobalContext(out IContextData? context)) {
            parentWindow = ITopLevel.FromContext(context);
        }

        if (parentWindow == null && alternateContext != null) {
            parentWindow = ITopLevel.FromContext(alternateContext);
        }

        return parentWindow;
    }
}