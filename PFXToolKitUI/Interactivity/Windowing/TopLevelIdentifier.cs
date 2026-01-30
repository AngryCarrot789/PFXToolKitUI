// 
// Copyright (c) 2026-2026 REghZy
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

namespace PFXToolKitUI.Interactivity.Windowing;

/// <summary>
/// Identifies a specific top level
/// </summary>
public readonly struct TopLevelIdentifier : IEquatable<TopLevelIdentifier> {
    /// <summary>
    /// Gets the unique id for the top level
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the instance number
    /// </summary>
    public ulong InstanceNumber { get; }
    
    private TopLevelIdentifier(string id, ulong instanceNumber) {
        this.Id = id;
        this.InstanceNumber = instanceNumber;
    }
    
    /// <summary>
    /// Creates an identifier for a top level that is only instantiated once or has a single/shared unique state
    /// </summary>
    /// <param name="id">The top level ID</param>
    /// <returns>The identifier</returns>
    public static TopLevelIdentifier Single(string id) => new TopLevelIdentifier(id, 0);
    
    /// <summary>
    /// Creates an identifier for a top level that can be instantiated multiple times
    /// </summary>
    /// <param name="id">The top level ID</param>
    /// <param name="instanceNumber">The instance number</param>
    /// <returns>The identifier</returns>
    public static TopLevelIdentifier Multi(string id, ulong instanceNumber) => new TopLevelIdentifier(id, instanceNumber);

    public bool Equals(TopLevelIdentifier other) {
        return this.Id == other.Id && this.InstanceNumber == other.InstanceNumber;
    }

    public override bool Equals(object? obj) {
        return obj is TopLevelIdentifier other && this.Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(this.Id, this.InstanceNumber);
    }

    public static bool operator ==(TopLevelIdentifier left, TopLevelIdentifier right) => left.Equals(right);

    public static bool operator !=(TopLevelIdentifier left, TopLevelIdentifier right) => !(left == right);

    public static void ThrowIfInvalid(TopLevelIdentifier topLevelIdentifier) {
        if (string.IsNullOrWhiteSpace(topLevelIdentifier.Id))
            throw new InvalidOperationException("Identifier's Id is null, empty or consists of only whitespaces");
    }
}