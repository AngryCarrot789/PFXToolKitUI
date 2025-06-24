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

using PFXToolKitUI.Themes;

namespace PFXToolKitUI.Icons;

public readonly struct GeometryEntry {
    public readonly string Geometry;
    public readonly IColourBrush? Fill;
    public readonly IColourBrush? Stroke;
    public readonly double StrokeThickness;

    public GeometryEntry(string geometry, IColourBrush? fill, IColourBrush? stroke, double strokeThickness) {
        this.Geometry = geometry;
        this.Fill = fill;
        this.Stroke = stroke;
        this.StrokeThickness = strokeThickness;
    }

    public GeometryEntry(string geometry, IColourBrush? fill) {
        this.Geometry = geometry;
        this.Fill = fill;
    }

    public GeometryEntry(string geometry) {
        this.Geometry = geometry;
    }
}