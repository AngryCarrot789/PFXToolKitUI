// 
// Copyright (c) 2023-2025 REghZy
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

using System.Globalization;
using System.Numerics;

namespace PFXToolKitUI.Themes.Gradients;

/// <summary>
/// Defines the reference point units of a <see cref="RelativePoint"/> 
/// </summary>
public enum RelativeUnit {
    /// <summary>
    /// The point is expressed as a fraction of the containing element's size.
    /// </summary>
    Relative,

    /// <summary>
    /// The point is absolute (i.e. in pixels).
    /// </summary>
    Absolute,
}

/// <summary>
/// Defines a point that may be defined relative to a containing element.
/// </summary>
public
    readonly struct RelativePoint : IEquatable<RelativePoint> {
    /// <summary>
    /// A point at the top left of the containing element.
    /// </summary>
    public static readonly RelativePoint TopLeft = new RelativePoint(0, 0, RelativeUnit.Relative);

    /// <summary>
    /// A point at the center of the containing element.
    /// </summary>
    public static readonly RelativePoint Center = new RelativePoint(0.5f, 0.5f, RelativeUnit.Relative);

    /// <summary>
    /// A point at the bottom right of the containing element.
    /// </summary>
    public static readonly RelativePoint BottomRight = new RelativePoint(1, 1, RelativeUnit.Relative);

    private readonly Vector2 _point;

    private readonly RelativeUnit _unit;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelativePoint"/> struct.
    /// </summary>
    /// <param name="x">The X point.</param>
    /// <param name="y">The Y point</param>
    /// <param name="unit">The unit.</param>
    public RelativePoint(float x, float y, RelativeUnit unit)
        : this(new Vector2(x, y), unit) {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RelativePoint"/> struct.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="unit">The unit.</param>
    public RelativePoint(Vector2 point, RelativeUnit unit) {
        this._point = point;
        this._unit = unit;
    }

    /// <summary>
    /// Gets the point.
    /// </summary>
    public Vector2 Point => this._point;

    /// <summary>
    /// Gets the unit.
    /// </summary>
    public RelativeUnit Unit => this._unit;

    /// <summary>
    /// Checks for equality between two <see cref="RelativePoint"/>s.
    /// </summary>
    /// <param name="left">The first point.</param>
    /// <param name="right">The second point.</param>
    /// <returns>True if the points are equal; otherwise false.</returns>
    public static bool operator ==(RelativePoint left, RelativePoint right) {
        return left.Equals(right);
    }

    /// <summary>
    /// Checks for inequality between two <see cref="RelativePoint"/>s.
    /// </summary>
    /// <param name="left">The first point.</param>
    /// <param name="right">The second point.</param>
    /// <returns>True if the points are unequal; otherwise false.</returns>
    public static bool operator !=(RelativePoint left, RelativePoint right) {
        return !left.Equals(right);
    }

    /// <summary>
    /// Checks if the <see cref="RelativePoint"/> equals another object.
    /// </summary>
    /// <param name="obj">The other object.</param>
    /// <returns>True if the objects are equal, otherwise false.</returns>
    public override bool Equals(object? obj) => obj is RelativePoint other && this.Equals(other);

    /// <summary>
    /// Checks if the <see cref="RelativePoint"/> equals another point.
    /// </summary>
    /// <param name="p">The other point.</param>
    /// <returns>True if the objects are equal, otherwise false.</returns>
    public bool Equals(RelativePoint p) {
        return this.Unit == p.Unit && this.Point == p.Point;
    }

    /// <summary>
    /// Gets a hashcode for a <see cref="RelativePoint"/>.
    /// </summary>
    /// <returns>A hash code.</returns>
    public override int GetHashCode() {
        unchecked {
            return (this._point.GetHashCode() * 397) ^ (int) this._unit;
        }
    }

    /// <summary>
    /// Converts a <see cref="RelativePoint"/> into pixels.
    /// </summary>
    /// <param name="size">The size of the visual.</param>
    /// <returns>The origin point in pixels.</returns>
    public Vector2 ToPixels(Vector2 size) {
        return this._unit == RelativeUnit.Absolute ? this._point : new Vector2(this._point.X * size.X, this._point.Y * size.Y);
    }

    /// <summary>
    /// Parses a <see cref="RelativePoint"/> string.
    /// </summary>
    /// <param name="s">The string.</param>
    /// <returns>The parsed <see cref="RelativePoint"/>.</returns>
    public static RelativePoint Parse(string s) {
        string[] parts = s.Split(',');
        if (parts.Length != 2) {
            throw new FormatException("Invalid point");
        }

        string x = parts[0];
        string y = parts[1];

        RelativeUnit unit = RelativeUnit.Absolute;
        float scale = 1.0f;

        if (x.EndsWith('%')) {
            if (!y.EndsWith('%')) {
                throw new FormatException("If one coordinate is relative, both must be.");
            }

            x = x.TrimEnd('%');
            y = y.TrimEnd('%');
            unit = RelativeUnit.Relative;
            scale = 0.01f;
        }

        return new RelativePoint(
            float.Parse(x, CultureInfo.InvariantCulture) * scale,
            float.Parse(y, CultureInfo.InvariantCulture) * scale,
            unit);
    }

    /// <summary>
    /// Returns a String representing this RelativePoint instance.
    /// </summary>
    /// <returns>The string representation.</returns>
    public override string ToString() {
        return this._unit == RelativeUnit.Absolute ? this._point.ToString() : string.Format(CultureInfo.InvariantCulture, "{0}%, {1}%", this._point.X * 100, this._point.Y * 100);
    }
}