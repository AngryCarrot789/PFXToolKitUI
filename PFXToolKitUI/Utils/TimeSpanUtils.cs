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

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace PFXToolKitUI.Utils;

/// <summary>
/// A helper class for parsing and formatting strings in quantities of hours, minutes, seconds and milliseconds
/// </summary>
public static class TimeSpanUtils {
    // Modified version of https://stackoverflow.com/a/47702311
    private static readonly Regex regex = new Regex(@"^\s*((?<hours>\d+)h\s*)?((?<minutes>\d+)m\s*)?((?<seconds>\d+)s\s*)?((?<millis>\d+)ms\s*)?((?<micros>\d+)us\s*)?$", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.RightToLeft);
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
        Group group;
        int? hrs = (group = m.Groups["hours"]).Success && int.TryParse(m.Groups["hours"].Value, out int tmpVal) ? tmpVal : null;
        if ((hrs.HasValue && hrs.Value < 0) || (group.Success && !hrs.HasValue)) {
            errorMessage = hrs.HasValue ? "Hours cannot be negative" : "Invalid Hours";
            return false;
        }

        int? mins = (group = m.Groups["minutes"]).Success && int.TryParse(m.Groups["minutes"].Value, out tmpVal) ? tmpVal : null;
        if ((mins.HasValue && mins.Value < 0) || (group.Success && !mins.HasValue)) {
            errorMessage = mins.HasValue ? "Minutes cannot be negative" : "Invalid Minutes";
            return false;
        }

        int? secs = (group = m.Groups["seconds"]).Success && int.TryParse(m.Groups["seconds"].Value, out tmpVal) ? tmpVal : null;
        if ((secs.HasValue && secs.Value < 0) || (group.Success && !secs.HasValue)) {
            errorMessage = secs.HasValue ? "Seconds cannot be negative" : "Invalid Seconds";
            return false;
        }

        int? millis = (group = m.Groups["millis"]).Success && int.TryParse(m.Groups["millis"].Value, out tmpVal) ? tmpVal : null;
        if ((millis.HasValue && millis.Value < 0) || (group.Success && !millis.HasValue)) {
            errorMessage = millis.HasValue ? "Milliseconds cannot be negative" : "Invalid Milliseconds";
            return false;
        }
        
        int? micros = (group = m.Groups["micros"]).Success && int.TryParse(m.Groups["micros"].Value, out tmpVal) ? tmpVal : null;
        if ((micros.HasValue && micros.Value < 0) || (group.Success && !micros.HasValue)) {
            errorMessage = micros.HasValue ? "Microseconds cannot be negative" : "Invalid Microseconds";
            return false;
        }

        if (!hrs.HasValue && !mins.HasValue && !secs.HasValue && !millis.HasValue && !micros.HasValue) {
            if (uint.TryParse(input, out uint value)) {
                errorMessage = null;
                timeSpan = TimeSpan.FromMilliseconds(value);
                return true;
            }

            errorMessage = "Invalid time format. Example: 4h3m2s1ms (4 hrs, 3 mins, 2 secs, 1 millisecond)";
            return false;
        }

        long totalMicros = (((long) (hrs ?? 0) * 3600 + (long) (mins ?? 0) * 60 + (secs ?? 0)) * 1000 + (millis ?? 0)) * 1000 + (micros ?? 0);
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
        double hours = span.Days * 24.0 + span.Hours;
        if (hours > 0)
            output.Add(Math.Round(hours, 1) + (useLongSuffix ? (" hour" + (DoubleUtils.AreClose(hours, 1.0) ? "" : "s")) : "h"));
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