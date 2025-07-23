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
using System.Text;

namespace PFXToolKitUI.Utils;

public static class StringUtils {
    public static string JSubstring(this string @this, int startIndex, int endIndex) {
        return @this.Substring(startIndex, endIndex - startIndex);
    }

    public static bool IsEmpty(this string @this) {
        return string.IsNullOrEmpty(@this);
    }

    public static string Join(string a, string b, char join) {
        return new StringBuilder(32).Append(a).Append(join).Append(b).ToString();
    }

    public static string Join(string a, string b, string c, char join) {
        return new StringBuilder(32).Append(a).Append(join).Append(b).Append(join).Append(c).ToString();
    }

    public static string? JoinString(this IEnumerable<string> elements, string delimiter, string finalDelimiter, string? emptyEnumerator = "") {
        using (IEnumerator<string> enumerator = elements.GetEnumerator()) {
            return JoinString(enumerator, delimiter, finalDelimiter, emptyEnumerator);
        }
    }

    public static string? JoinString(this IEnumerator<string> elements, string delimiter, string finalDelimiter, string? emptyEnumerator = "") {
        if (!elements.MoveNext()) {
            return emptyEnumerator;
        }

        StringBuilder sb = new StringBuilder();
        sb.Append(elements.Current);
        if (elements.MoveNext()) {
            string last = elements.Current;
            while (elements.MoveNext()) {
                sb.Append(delimiter).Append(last);
                last = elements.Current;
            }

            sb.Append(finalDelimiter).Append(last);
        }

        return sb.ToString();
    }

    public static string Repeat(char ch, int count) {
        // char[] chars = new char[count];
        // for (int i = 0; i < count; i++)
        //     chars[i] = ch;
        // return new string(chars);

        // C# has an optimised version I guess...
        return count == 0 ? string.Empty : new String(ch, count);
    }

    public static string Repeat(string str, int count) {
        StringBuilder sb = new StringBuilder(str.Length * count);
        for (int i = 0; i < count; i++)
            sb.Append(str);
        return sb.ToString();
    }

    /// <summary>
    /// Tries to pad or trim the string based on a fixed character limit, padding using the given 'fit' character.
    /// The trim is a hard cut without any ellipse, and the pad just inserts the fit character
    /// </summary>
    /// <param name="str">The string to pad or trim</param>
    /// <param name="length">The character limit</param>
    /// <param name="fit">A padding character</param>
    /// <returns>The padded, trimmed or original string</returns>
    public static string FitLength(this string str, int length, char fit = ' ') {
        int strlen = str.Length;
        if (strlen > length) {
            return str.Substring(0, length);
        }
        else if (strlen < length) {
            return str + Repeat(fit, length - strlen);
        }
        else {
            return str;
        }
    }

    public static bool EqualsIgnoreCase(this string @this, string? value) {
        return @this.Equals(value, StringComparison.OrdinalIgnoreCase);
    }

    public static string RemoveChar(this string @this, char ch) {
        int index = @this.IndexOf(ch);
        if (index == -1)
            return @this;

        int count = @this.Length;
        StringBuilder sb = new StringBuilder(count);
        sb.Append(@this, 0, index);

        for (int i = index + 1; i < count; i++) {
            char character = @this[i];
            if (character != ch) {
                sb.Append(character);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Split the input string by the given splitter, and provide the left and right values as out parameters (splitter is not included
    /// </summary>
    /// <param name="this">Value to split</param>
    /// <param name="splitter">Split value</param>
    /// <param name="a">Everything before the splitter</param>
    /// <param name="b">Everything after the splitter</param>
    /// <returns>True if the string contained the splitter, otherwise false</returns>
    public static bool Split(this string @this, string splitter, [NotNullWhen(true)] out string? a, [NotNullWhen(true)] out string? b) {
        int index;
        if (string.IsNullOrEmpty(@this) || (index = @this.IndexOf(splitter)) < 0) {
            a = b = null;
            return false;
        }

        a = @this.Substring(0, index);
        b = @this.Substring(index + splitter.Length);
        return true;
    }

    // https://stackoverflow.com/a/40041591/11034928
    public static int Digit(char value, int radix) {
        if (radix <= 0 || radix > 36)
            return -1; // Or throw exception
        if (radix <= 10)
            return value >= '0' && value < '0' + radix ? value - '0' : -1;
        else if (value >= '0' && value <= '9')
            return value - '0';
        else if (value >= 'a' && value < 'a' + radix - 10)
            return value - 'a' + 10;
        else if (value >= 'A' && value < 'A' + radix - 10)
            return value - 'A' + 10;
        return -1;
    }

    public static string SplitLast(string str, char ch) {
        int index = str.LastIndexOf(ch);
        return index == -1 ? str : str.Substring(index + 1);
    }

    public static string InjectOrUseChars(string? src, int srcIndex, Span<char> arg) {
        if (src == null) {
            return new string(arg);
        }
        else {
            char[] chars = new char[src.Length + arg.Length];
            src.CopyTo(0, chars, 0, srcIndex);
            for (int i = 0; i < arg.Length; i++)
                chars[srcIndex + i] = arg[i];
            int j = srcIndex + arg.Length;
            src.CopyTo(srcIndex, chars, j, chars.Length - j);
            return new string(chars);
        }
    }

    public static void ValidateNotWhiteSpaces(string str, string varName) {
        if (str == null)
            throw new ArgumentNullException(varName);
        if (string.IsNullOrWhiteSpace(str))
            throw new ArgumentException(varName + " cannot be null, empty or consist of only whitespaces", varName);
    }

    public static string? JoinEx<T>(IList<T> items, string? delimiter, string finalDelimiter, string? emptyValue, Func<T, string> toStringFunc) {
        int size = items.Count;
        if (size <= 0)
            return emptyValue;
        if (size == 1)
            return toStringFunc(items[0]);
        if (delimiter == null) {
            delimiter = ", ";
        }

        StringBuilder sb = new StringBuilder(size * 4);
        sb.Append(toStringFunc(items[0]));
        for (int i = 1, end = (size - 1); i < end; i++) {
            sb.Append(delimiter).Append(toStringFunc(items[i]));
        }

        return sb.Append(finalDelimiter).Append(toStringFunc(items[size - 1])).ToString();
    }
}