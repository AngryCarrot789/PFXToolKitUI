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

namespace PFXToolKitUI.Utils.Events;

public static class MulticastUtils {
    /// <summary>
    /// NOT THREAD SAFE
    /// </summary>
    public static void AddWithProxy<TDelegate, TState>(ref TDelegate? handlerList, TDelegate handler, TState state, Action<TState> attachProxy) where TDelegate : Delegate? where TState : class {
        if (handler == null)
            return;
        if (handlerList == null)
            attachProxy(state); // Add() will cause handlerList to become non-null

        Add(ref handlerList, handler);
    }

    /// <summary>
    /// NOT THREAD SAFE
    /// </summary>
    public static void RemoveWithProxy<TDelegate, TState>(ref TDelegate? handlerList, TDelegate handler, TState state, Action<TState> detachProxy) where TDelegate : Delegate? where TState : class {
        if (handlerList == null || handler == null)
            return;
        if (handlerList.HasSingleTarget)
            detachProxy(state); // Remove() will cause handlerList to become null
        
        Remove(ref handlerList, handler);
    }

    public static void Add<TDelegate>(ref TDelegate? x, TDelegate handler) where TDelegate : Delegate? {
        TDelegate? curr = x, prev;
        do {
            prev = curr;
            curr = Interlocked.CompareExchange(ref x, (TDelegate?) Delegate.Combine(prev, handler), prev);
        } while (curr != prev);
    }

    public static void Remove<TDelegate>(ref TDelegate? x, TDelegate handler) where TDelegate : Delegate? {
        TDelegate? curr = x, prev;
        do {
            prev = curr;
            curr = Interlocked.CompareExchange(ref x, (TDelegate?) Delegate.Remove(prev, handler), prev);
        } while (curr != prev);
    }
}