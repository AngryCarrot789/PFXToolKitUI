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

namespace PFXToolKitUI.Interactivity.Contexts;

public abstract class MultiChangeToken : IDisposable {
    public readonly IMutableContextData Context;
    private bool disposed;

    protected MultiChangeToken(IMutableContextData context) {
        this.Context = context;
    }

    /// <summary>
    /// Disposes this token
    /// </summary>
    public void Dispose() {
        ObjectDisposedException.ThrowIf(this.disposed, this);
        this.disposed = true;
        this.OnDisposed();
    }

    protected abstract void OnDisposed();
}