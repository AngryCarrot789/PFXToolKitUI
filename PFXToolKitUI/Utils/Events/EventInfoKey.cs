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

public readonly struct EventInfoKey(Type ownerType, string eventName) : IEquatable<EventInfoKey> {
    public readonly Type OwnerType = ownerType;
    public readonly string EventName = eventName;
    public bool Equals(EventInfoKey other) => this.OwnerType == other.OwnerType && this.EventName == other.EventName;
    public override bool Equals(object? obj) => obj is EventInfoKey other && this.Equals(other);
    public override int GetHashCode() => HashCode.Combine(this.OwnerType, this.EventName);
    public override string ToString() => $"{this.OwnerType.Name}#{this.EventName}";

    public static bool operator ==(EventInfoKey left, EventInfoKey right) => left.Equals(right);

    public static bool operator !=(EventInfoKey left, EventInfoKey right) => !(left == right);
}