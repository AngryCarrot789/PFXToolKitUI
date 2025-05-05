// 
// Copyright (c) 2024-2025 REghZy
// 
// This file is part of FramePFX.
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
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace PFXToolKitUI.Utils;

public static class NumberUtils {
    // Ladies and gentlemen, what the fuck     |    NOT NULL NULLABLE??   |
    private static void NumberStyleFromIntInput([NotNull] ref string? input, out NumberStyles style) {
        if (input != null && input.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase)) {
            input = input.Substring(2);
            style = NumberStyles.HexNumber;
        }
        else {
            style = NumberStyles.Integer;
        }
    }
    
    public static bool TryParseHexOrRegular<T>(string? input, out T result) where T : struct, IBinaryInteger<T> {
        NumberStyleFromIntInput(ref input, out NumberStyles style);
        return T.TryParse(input, style, null, out result);
    }
    
    public static bool TryParseHexOrRegular<T>(string? input, IFormatProvider? provider, out T result) where T : struct, IBinaryInteger<T> {
        NumberStyleFromIntInput(ref input, out NumberStyles style);
        return T.TryParse(input, style, provider, out result);
    }
    
    public static T ParseHexOrRegular<T>(string input, IFormatProvider? provider = null) where T : struct, IBinaryInteger<T> {
        NumberStyleFromIntInput(ref input, out NumberStyles style);
        return T.Parse(input, style, provider);
    }
}