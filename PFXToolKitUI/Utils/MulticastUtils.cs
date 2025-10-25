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

namespace PFXToolKitUI.Utils;

public static class MulticastUtils {
    public static void AddProxy<TDelegate, TState>(ref int count, ref TDelegate? backingEvent, TDelegate value, TState state, Action<TState> attach) where TDelegate : Delegate? {
        if (Interlocked.Increment(ref count) == 1) {
            attach(state);
        }

        Add(ref backingEvent, value);
    }

    public static void RemoveProxy<TDelegate, TState>(ref int count, ref TDelegate? backingEvent, TDelegate value, TState state, Action<TState> detach) where TDelegate : Delegate? {
        Remove(ref backingEvent, value);
        if (Interlocked.Decrement(ref count) == 0) {
            detach(state);
        }
    }

    public static void Add<TDelegate>(ref TDelegate? src, TDelegate value) where TDelegate : Delegate? {
        TDelegate? a = src, b;
        do {
            b = a;
            a = Interlocked.CompareExchange(ref src, (TDelegate?) Delegate.Combine(b, value), b);
        } while (a != b);
    }

    public static void Remove<TDelegate>(ref TDelegate? src, TDelegate value) where TDelegate : Delegate? {
        TDelegate? a = src, b;
        do {
            b = a;
            a = Interlocked.CompareExchange(ref src, (TDelegate?) Delegate.Remove(b, value), b);
        } while (a != b);
    }
}