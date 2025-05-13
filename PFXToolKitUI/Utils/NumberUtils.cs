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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PFXToolKitUI.Utils;

public static class NumberUtils { 
    private static readonly char[] HEX_CHARS = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'];
    public static readonly byte[] HEX_CHARS_ASCII = "0123456789ABCDEF"u8.ToArray();
    
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int HexCharToInt(char c) => c <= '9' ? (c - '0') : ((c & ~0x20 /* LOWER TO UPPER CASE */) - 'A' + 10);

    /// <summary>
    /// Writes the ascii characters of the hexadecimal representation of <see cref="value"/> into <see cref="dstAsciiChars"/> (starting at <see cref="offset"/>) 
    /// </summary>
    /// <param name="value">The value to convert to hex</param>
    /// <param name="dstAsciiChars">The destination character buffer</param>
    /// <param name="offset">The starting offset in <see cref="dstAsciiChars"/></param>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void UInt32ToHexAscii(uint value, ref byte dstAsciiChars, int offset) {
        ref byte asciiHexChars = ref MemoryMarshal.GetArrayDataReference(HEX_CHARS_ASCII);
        for (int j = 7; j >= 0; j--, value >>= 4) {
            Unsafe.AddByteOffset(ref dstAsciiChars, j + offset) = Unsafe.ReadUnaligned<byte>(ref Unsafe.AddByteOffset(ref asciiHexChars, value & 0xF));
        }
    }
}