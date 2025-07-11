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

namespace PFXToolKitUI.Interactivity.Formatting;

/// <summary>
/// A value formatter that converts a unit value (0.0 to 1.0) into a percentage (0 to 100%) with an optional decimal precision
/// </summary>
public class PrefixValueFormatter : BaseSimpleValueFormatter {
    private string? prefix;

    public string? Prefix {
        get => this.prefix;
        set {
            if (this.prefix != value) {
                this.prefix = value;
                this.OnInvalidateFormat();
            }
        }
    }

    public PrefixValueFormatter(string? prefix = null, int nonEditingRoundedPlaces = 2, int editingRoundedPlaces = 6) {
        this.prefix = prefix;
        this.NonEditingRoundedPlaces = nonEditingRoundedPlaces;
        this.EditingRoundedPlaces = editingRoundedPlaces;
    }

    public override string ToString(double value, bool isEditing) {
        return value.ToString(isEditing ? this.EditingRoundedPlacesFormat : this.NonEditingRoundedPlacesFormat) + (this.prefix ?? "");
    }

    public override bool TryConvertToDouble(string format, out double value) {
        int i = 0, j = format.Length;
        if (!string.IsNullOrEmpty(this.prefix) && format.StartsWith(this.prefix)) {
            i += this.prefix.Length;
        }

        if (i >= j) {
            value = default;
            return false;
        }

        return double.TryParse(format.AsSpan(i, j - i), out value);
    }

    public static PrefixValueFormatter Parse(string input) {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input is null, empty or whitespaces only", nameof(input));

        string[] parts = input.Split(',');
        if (parts.Length != 3)
            throw new ArgumentException("Missing 3 parts split by ',' character between the non-editing and editing rounded places", nameof(input));

        if (!int.TryParse(parts[0], out int nonEditingPlaces))
            throw new ArgumentException($"Invalid integer for non-editing part '{parts[0]}'", nameof(input));

        if (!int.TryParse(parts[1], out int editingPlaces))
            throw new ArgumentException($"Invalid integer for non-editing part '{parts[1]}'", nameof(input));

        return new PrefixValueFormatter(parts[2], nonEditingPlaces, editingPlaces);
    }
}