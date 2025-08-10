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

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace PFXToolKitUI.Utils;

public static class NumberUtils {
    private static readonly char[] HEX_CHARS = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'];
    public static readonly byte[] HEX_CHARS_ASCII = "0123456789ABCDEF"u8.ToArray();

    private static void NumberStyleFromIntInput(ref string? input, out NumberStyles style) {
        if (input != null) {
            if (input.StartsWith("0x", StringComparison.Ordinal))
                input = input.Substring(2);
            else if (input.StartsWith("-0x", StringComparison.Ordinal))
                input = input.Substring(3);
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

    public static T ParseHexOrRegular<T>(string? input, IFormatProvider? provider = null) where T : struct, IBinaryInteger<T> {
        NumberStyleFromIntInput(ref input, out NumberStyles style);
        return T.Parse(input!, style, provider);
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

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static string BytesToHexAscii(ReadOnlySpan<byte> srcBuffer, char? join = ' ') {
        if (srcBuffer.Length < 1) {
            return "";
        }

        char[] dstBuffer = new char[(srcBuffer.Length << 1) + (join.HasValue ? (srcBuffer.Length - 1) : 0)];
        for (int i = 0, j = 0, ichLast = dstBuffer.Length - 2, incr = join.HasValue ? 3 : 2; i < srcBuffer.Length; i++, j += incr) {
            byte b = srcBuffer[i];
            dstBuffer[j + 0] = HEX_CHARS[(b >> 4) & 0xF];
            dstBuffer[j + 1] = HEX_CHARS[b & 0xF];
            if (j != ichLast && join.HasValue)
                dstBuffer[j + 2] = join.Value;
        }

        return new string(dstBuffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static bool TryParseHexAsciiToBytes(ReadOnlySpan<char> srcText, [NotNullWhen(true)] out byte[]? bytes, char? join = ' ') {
        if (srcText.Length < 1) {
            bytes = [];
            return true;
        }

        // [FF FF FF FF FF FF] C = 6, WSC = 5, HCH = 12, LEN = 17, WSC+C=11
        int cchJoin = join.HasValue ? srcText.Count(join.Value) : 0;
        byte[] dstBuffer = new byte[(srcText.Length - cchJoin) >> 1];
        for (int i = 0, j = 0, incr = join.HasValue ? 3 : 2; i < srcText.Length; i += incr, j++) {
            char ch1 = srcText[i + 0];
            char ch2 = srcText[i + 1];
            if (!IsCharValidHex(ch1) || !IsCharValidHex(ch2)) {
                bytes = null;
                return false;
            }

            dstBuffer[j] = (byte) ((HexCharToInt(ch1) << 4) | HexCharToInt(ch2));
        }

        bytes = dstBuffer;
        return true;
    }

    public static bool IsCharValidHex(char c) => StringUtils.Digit(c, 16) != -1;

    public static string ConvertStringToHex(string input, Encoding encoding) {
        byte[] bytes = encoding.GetBytes(input);
        StringBuilder sb = new StringBuilder(bytes.Length * 2);
        foreach (byte b in bytes)
            sb.Append(b.ToString("X2"));
        return sb.ToString();
    }
}