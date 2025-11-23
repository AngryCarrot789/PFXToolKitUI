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

using System.Diagnostics;
using PFXToolKitUI.EventHelpers;

namespace PFXToolKitUI.Avalonia.Bindings;

internal static class EventRelayBinderUtils {
    [Conditional("DEBUG")]
    public static void CheckMemoryLeaksOnAppShutdown() {
#if DEBUG
        List<KeyValuePair<object, IEnumerable<KeyValuePair<string, IRelayEventHandler[]>>>> entries = EventRelayStorage.UIStorage.DEBUG_AttachmentMap.ToList();
        if (entries.Count < 1) {
            return;
        }

        Debug.WriteLine("Warning: binder memory leaks present (models still attached)");
        foreach (KeyValuePair<object, IEnumerable<KeyValuePair<string, IRelayEventHandler[]>>> entry in entries) {
            Debug.WriteLine($"Model Type: {entry.Key.GetType().FullName}");
            foreach (KeyValuePair<string, IRelayEventHandler[]> subEntry in entry.Value) {
                Debug.WriteLine($"   Event Name: {subEntry.Key}");
                Debug.WriteLine("   Handler(s) (most likely the binder classes):");
                foreach (IRelayEventHandler handler in subEntry.Value) {
                    Debug.WriteLine($"      - {handler.GetType().FullName}");
                    if (handler is IBinder binder && binder.IsFullyAttached) {
                        Debug.WriteLine($"        (attached to {binder.Debug_Control?.GetType().FullName})");
                    }
                }
            }
        }

        Debugger.Break();
#endif
    }
}