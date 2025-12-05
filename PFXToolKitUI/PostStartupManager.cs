// 
// Copyright (c) 2025-2025 REghZy
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

using System.Diagnostics;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI;

/// <summary>
/// Manages post-started actions
/// </summary>
public sealed class PostStartupManager {
    public static PostStartupManager Instance => ApplicationPFX.GetComponent<PostStartupManager>();
    
    private List<Action>? actions = new List<Action>();

    public PostStartupManager() {
    }

    internal void OnPostStartup() {
        List<Action>? list = Interlocked.Exchange(ref this.actions, null);
        Debug.Assert(list != null);
        foreach (Action action in list) {
            try {
                action();
            }
            catch (Exception e) {
                AppLogger.Instance.WriteLine("Exception while executing post-startup actions: " + e.GetToString());
            }
        }
    }
    
    public void Register(Action action) {
        if (this.actions == null)
            throw new InvalidOperationException("Post-startup actions already invoked");
        this.actions.Add(action);
    }
}