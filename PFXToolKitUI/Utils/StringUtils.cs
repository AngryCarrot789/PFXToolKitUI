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
    public static bool Split(this string @this, string splitter, out string a, out string b) {
        int index;
        if (string.IsNullOrEmpty(@this) || (index = @this.IndexOf(splitter)) < 0) {
            a = b = null;
            return false;
        }

        a = @this.Substring(0, index);
        b = @this.Substring(index + splitter.Length);
        return true;
    }

    // remake of Format for explicitly 1 parameter
    // okay{0}then
    public static string InjectFormat(string format, string argument) {
        int i;
        if (format == null || (i = format.IndexOf('{', 0)) == -1)
            throw new Exception("Missing '{0}' somewhere in the source string");
        char[] chars = new char[format.Length + argument.Length - 3];
        format.CopyTo(0, chars, 0, i);
        argument.CopyTo(0, chars, i, argument.Length);
        format.CopyTo(i + 3, chars, i + argument.Length, chars.Length - (i + argument.Length));
        return new string(chars);
    }

    // Took this from a minecraft plugin I made, because java's built in format function was annoying to
    // use so I made my own that outperformed it by about 2x... not to toot my own horn or anything ;)

    private static readonly object[] EmptyArray = new object[0];

    public static String Format(String format, params Object[] args) {
        int i, j, k, num;
        if (format == null || (i = format.IndexOf('{', j = 0)) == -1)
            return format;
        if (args == null)
            args = EmptyArray ?? new object[0];

        // buffer of 2x format is typically the best result
        FastStringBuf sb = new FastStringBuf(format.Length * 2);
        do {
            if (i == 0 || format[i - 1] != '\\') {
                // check escape char
                sb.append(format, j, i); // append text between j and before '{' char
                if ((k = format.IndexOf('}', i)) != -1) {
                    // get closing char index
                    j = k + 1; // set last char to after closing char
                    if ((num = ParseIntSigned(format, i + 1, k, 10)) >= 0 && num < args.Length) {
                        sb.append(args[num]); // append content
                    }
                    else {
                        // OLD: sb.append('{').append(format, i + 1, k).append('}');
                        sb.append(format, i, j); // append values between { and }
                    }

                    i = k; // set next search index to the '}' char
                }
                else {
                    j = i; // set last char to the last '{' char
                }
            }
            else {
                // remove escape char
                sb.append(format, j, i - 1); // append text between last index and before the escape char
                j = i; // set last index to the '{' char, which was originally escaped
            }
        } while ((i = format.IndexOf('{', i + 1)) != -1);

        sb.append(format, j, format.Length); // append remainder of string
        return sb.ToString();
    }

    // Try parse non-negative int, or return -1 on failure
    public static int ParseIntSigned(string chars, int index, int endIndex, int radix) {
        if (index < 0 || endIndex <= index) {
            return -1;
        }

        char first = chars[index];
        if (first < '0') {
            // Possible leading "+"
            if (first != '+' || (endIndex - index) == 1)
                return -1; // Cannot have lone "+"
            index++;
        }

        int result = 0;
        const int limit = -int.MaxValue; // Integer.MIN_VALUE + 1
        int radixMinLimit = limit / radix;
        while (index < endIndex) {
            // Accumulating negatively avoids surprises near MAX_VALUE
            int digit = Digit(chars[index++], radix);
            if (digit < 0 || result < radixMinLimit)
                return -1;
            if ((result *= radix) < limit + digit)
                return -1;
            result -= digit;
        }

        return -result;
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