//
// Copyright (c) 2024-2025 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace PFXToolKitUI.Utils;

/// <summary>
/// A helper class for parsing and formatting strings in quantities of hours, minutes, seconds and milliseconds
/// </summary>
public static class TimeSpanUtils {
    // Modified version of https://stackoverflow.com/a/47702311
    private static readonly Regex regex = new Regex(@"^\s*((?<hours>\d+)h\s*)?((?<minutes>\d+)m\s*)?((?<seconds>\d+)s\s*)?((?<millis>\d+)ms\s*)?$", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.RightToLeft);
    public const long MaxMicros = long.MaxValue / TimeSpan.TicksPerMicrosecond;
    public const long MinMicros = long.MinValue / TimeSpan.TicksPerMicrosecond;

    public static bool IsOutOfRangeForDelay(TimeSpan timeSpan, [NotNullWhen(true)] out string? errorMessage, bool allowInfinite = false) {
        double totalMs = timeSpan.TotalMilliseconds;
        if (!allowInfinite || (long) totalMs != -1) {
            if (totalMs < 0) {
                errorMessage = "Negative time is not allowed";
                return true;
            }

            if (totalMs >= uint.MaxValue) {
                errorMessage = "Duration is too large";
                return true;
            }
        }

        errorMessage = null;
        return false;
    }

    public static bool TryParseTime(string input, out TimeSpan timeSpan, [NotNullWhen(false)] out string? errorMessage, long minMicros = 0, long maxMicros = MaxMicros) {
        timeSpan = TimeSpan.Zero;

        Match m = regex.Match(input);
        int? hrs = m.Groups["hours"].Success ? int.Parse(m.Groups["hours"].Value) : null;
        if (hrs.HasValue && hrs.Value < 0) {
            errorMessage = "Hours cannot be negative";
            return false;
        }

        int? mins = m.Groups["minutes"].Success ? int.Parse(m.Groups["minutes"].Value) : null;
        if (mins.HasValue && mins.Value < 0) {
            errorMessage = "Minutes cannot be negative";
            return false;
        }

        int? secs = m.Groups["seconds"].Success ? int.Parse(m.Groups["seconds"].Value) : null;
        if (secs.HasValue && secs.Value < 0) {
            errorMessage = "Seconds cannot be negative";
            return false;
        }

        int? millis = m.Groups["millis"].Success ? int.Parse(m.Groups["millis"].Value) : null;
        if (millis.HasValue && millis.Value < 0) {
            errorMessage = "Milliseconds cannot be negative";
            return false;
        }

        if (!hrs.HasValue && !mins.HasValue && !secs.HasValue && !millis.HasValue) {
            if (NumberUtils.TryParseHexOrRegular(input, out uint value)) {
                errorMessage = null;
                timeSpan = TimeSpan.FromMilliseconds(value);
                return true;
            }

            errorMessage = "Invalid time format. Example: 4h3m2s1ms (4 hrs, 3 mins, 2 secs, 1 millisecond)";
            return false;
        }

        long totalMicros = (((long) (hrs ?? 0) * 3600 + (long) (mins ?? 0) * 60 + (secs ?? 0)) * 1000 + (millis ?? 0)) * 1000;
        if (totalMicros < minMicros || totalMicros < MinMicros) {
            errorMessage = minMicros >= 0 ? "Negative time is not allowed" : "Duration is too small. Minimum is " + (Math.Max(minMicros, MinMicros) / 1000) + " milliseconds.";
            return false;
        }

        if (totalMicros > maxMicros || totalMicros > MaxMicros) {
            errorMessage = "Duration is too large. Maximum is " + (Math.Min(maxMicros, MaxMicros) / 1000) + " milliseconds.";
            return false;
        }

        errorMessage = null;
        timeSpan = new TimeSpan(totalMicros * TimeSpan.TicksPerMicrosecond);
        return true;
    }

    public static string ConvertToString(TimeSpan span, bool useLongSuffix = false, string join = " ") {
        return string.Join(join, ConvertToStringList(span, useLongSuffix));
    }

    public static List<string> ConvertToStringList(TimeSpan span, bool useLongSuffix) {
        List<string> output = new List<string>();
        long hours = ((long) span.Days * 24) + span.Hours;
        if (hours > 0)
            output.Add(hours + (useLongSuffix ? (" hour" + (hours == 1 ? "" : "s")) : "h"));
        if (span.Minutes > 0)
            output.Add(span.Minutes + (useLongSuffix ? (" minute" + Lang.S(span.Minutes)) : "m"));
        if (span.Seconds > 0)
            output.Add(span.Seconds + (useLongSuffix ? (" second" + Lang.S(span.Seconds)) : "s"));
        if (span.Milliseconds > 0)
            output.Add(span.Milliseconds + (useLongSuffix ? (" milli" + Lang.S(span.Milliseconds)) : "ms"));
        if (span.Microseconds > 0)
            output.Add(span.Microseconds + (useLongSuffix ? (" micro" + Lang.S(span.Microseconds)) : "us"));

        if (output.Count < 1)
            output.Add("0");

        return output;
    }
}