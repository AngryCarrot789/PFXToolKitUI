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

using System.Buffers;

namespace PFXToolKitUI.Utils;

public static class ArrayPools {
    public static Token<T> Rent<T>(int minimumLength, out T[] array, ArrayPool<T>? pool = null) {
        array = (pool ??= ArrayPool<T>.Shared).Rent(minimumLength);
        return new Token<T>(pool, array);
    }
    
    public static Token<T> RentSpan<T>(int minimumLength, out Span<T> span, ArrayPool<T>? pool = null) {
        T[] array = (pool ??= ArrayPool<T>.Shared).Rent(minimumLength);
        span = array.AsSpan(0, minimumLength);
        return new Token<T>(pool, array);
    }

    public readonly struct Token<T>(ArrayPool<T> arrayPool, T[] buffer) : IDisposable {
        public void Dispose() {
            arrayPool.Return(buffer);
        }
    }
}