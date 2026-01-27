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

namespace PFXToolKitUI.Utils.Reactive;

/// <summary>
/// Represents an observable object
/// </summary>
public interface IEventObservable<T> {
    /// <summary>
    /// Adds a subscriber to the event
    /// </summary>
    /// <param name="owner">The instance to add the event handler to</param>
    /// <param name="state">The state passed to the callback</param>
    /// <param name="callback">The callback</param>
    /// <param name="invokeImmediately">Immediately calls the callback before this method returns</param>
    /// <returns>The subscription to the event. Dispose to unsubscribe</returns>
    IDisposable Subscribe(T owner, object? state, EventHandler<T, object?> callback, bool invokeImmediately = true);
}