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

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.PropertyEditing.DataTransfer.Enums;

/// <summary>
/// Information about an enum set
/// </summary>
/// <typeparam name="TEnum">Type of enum</typeparam>
public class DataParameterEnumInfo<TEnum> where TEnum : struct, Enum {
    public static readonly TEnum DefaultValue;
    public static readonly TEnum MinValue, MaxValue;
    public static readonly ReadOnlyCollection<TEnum> EnumValues;
    public static readonly IReadOnlySet<TEnum> EnumValuesSet;
    public static readonly ReadOnlyCollection<TEnum> EnumValuesOrderedByName;

    /// <summary>
    /// Returns a list of allowed enum values which are also mapped to a readable string name
    /// </summary>
    public readonly ReadOnlyCollection<(TEnum, string)> AllowedEnumList;

    /// <summary>
    /// Returns a dictionary which maps enum values to a string
    /// </summary>
    public readonly IReadOnlyDictionary<TEnum, string> EnumToText;

    /// <summary>
    /// Returns a dictionary which maps a string to an enum
    /// </summary>
    public readonly IReadOnlyDictionary<string, TEnum> TextToEnum;

    /// <summary>
    /// Returns a list of allowed enum values
    /// </summary>
    public ReadOnlyCollection<TEnum> EnumList { get; }

    /// <summary>
    /// Returns a list of allowed enums, as their readable string name
    /// </summary>
    public ReadOnlyCollection<string> TextList { get; }

    private DataParameterEnumInfo(IEnumerable<TEnum> allowedEnumValues) {
        this.AllowedEnumList = allowedEnumValues.Select(x => (x, x.ToString())).ToList().AsReadOnly();
        this.EnumToText = new Dictionary<TEnum, string>(this.AllowedEnumList.Select(x => new KeyValuePair<TEnum, string>(x.Item1, x.Item2)));
        this.TextToEnum = new Dictionary<string, TEnum>(this.AllowedEnumList.Select(x => new KeyValuePair<string, TEnum>(x.Item2, x.Item1)));
        this.EnumList = this.AllowedEnumList.Select(x => x.Item1).ToList().AsReadOnly();
        this.TextList = this.AllowedEnumList.Select(x => x.Item2).ToList().AsReadOnly();
    }

    private DataParameterEnumInfo(IEnumerable<TEnum> allowedEnumValues, IReadOnlyDictionary<TEnum, string> enumToTextMap) {
        ArgumentNullException.ThrowIfNull(enumToTextMap);

        this.AllowedEnumList = allowedEnumValues.Select(x => (x, enumToTextMap.TryGetValue(x, out string? value) ? value : x.ToString())).ToList().AsReadOnly();

        // Generate missing translations
        Dictionary<TEnum, string> fullEnumToTextMap = new Dictionary<TEnum, string>(enumToTextMap);
        foreach ((TEnum theEnum, string theName) t in this.AllowedEnumList) {
            if (!fullEnumToTextMap.ContainsKey(t.theEnum))
                fullEnumToTextMap[t.theEnum] = t.theName;
        }

        this.EnumToText = fullEnumToTextMap.AsReadOnly();
        this.TextToEnum = new Dictionary<string, TEnum>(this.EnumToText.Select(x => new KeyValuePair<string, TEnum>(x.Value, x.Key)));
        this.EnumList = this.AllowedEnumList.Select(x => x.Item1).ToList().AsReadOnly();
        this.TextList = this.AllowedEnumList.Select(x => x.Item2).ToList().AsReadOnly();
    }

    static DataParameterEnumInfo() {
        DefaultValue = default;
        EnumValues = Enum.GetValues<TEnum>().ToList().AsReadOnly();
        EnumValuesSet = new HashSet<TEnum>(EnumValues);
        EnumValuesOrderedByName = EnumValues.OrderBy(x => x.ToString()).ToList().AsReadOnly();

        if (EnumValues.Count < 1) {
            MinValue = MaxValue = DefaultValue;
        }
        else {
            Type type = Enum.GetUnderlyingType(typeof(TEnum));
            if (type == typeof(byte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong)) {
                TEnum value = EnumValues[0];
                ulong val64 = Unsafe.As<TEnum, ulong>(ref value);
                
                TEnum eMin = value, eMax = value;
                ulong iMin = val64, iMax = val64;
                for (int i = 1; i < EnumValues.Count; i++) {
                    value = EnumValues[i];
                    val64 = Unsafe.As<TEnum, ulong>(ref value);
                    if (val64 < iMin) {
                        iMin = val64;
                        eMin = value;
                    }
                    else if (val64 > iMax) {
                        iMax = val64;
                        eMax = value;
                    }
                }

                MinValue = eMin;
                MaxValue = eMax;
            }
            else {
                // Apparently float and double types are technically supported but not compilable
                Debug.Assert(type == typeof(sbyte) || type == typeof(short) || type == typeof(int) || type == typeof(long));
                TEnum value = EnumValues[0];
                long val64 = Unsafe.As<TEnum, long>(ref value);
                
                TEnum eMin = value, eMax = value;
                long iMin = val64, iMax = val64;
                for (int i = 1; i < EnumValues.Count; i++) {
                    value = EnumValues[i];
                    val64 = Unsafe.As<TEnum, long>(ref value);
                    if (val64 < iMin) {
                        iMin = val64;
                        eMin = value;
                    }
                    else if (val64 > iMax) {
                        iMax = val64;
                        eMax = value;
                    }
                }

                MinValue = eMin;
                MaxValue = eMax;
            }
        }
    }

    /// <summary>
    /// Returns enum info for all enum constants of the enum type
    /// </summary>
    public static DataParameterEnumInfo<TEnum> All() {
        return new DataParameterEnumInfo<TEnum>(EnumValues);
    }

    /// <summary>
    /// Returns enum info for all enum constants of the enum type, with enum to readable name mappings
    /// </summary>
    /// <param name="enumToTextMap"></param>
    public static DataParameterEnumInfo<TEnum> All(IReadOnlyDictionary<TEnum, string> enumToTextMap) {
        return new DataParameterEnumInfo<TEnum>(EnumValues, enumToTextMap);
    }

    /// <summary>
    /// Returns enum info for the list of allowed enum values
    /// </summary>
    /// <param name="allowedEnumValues">The allowed enums</param>
    public static DataParameterEnumInfo<TEnum> FromAllowed(IEnumerable<TEnum> allowedEnumValues) {
        return new DataParameterEnumInfo<TEnum>(allowedEnumValues.Distinct());
    }

    /// <summary>
    /// Returns enum info for the list of allowed enum values, with enum to readable name mappings
    /// </summary>
    /// <param name="allowedEnumValues">The allowed enums</param>
    public static DataParameterEnumInfo<TEnum> FromAllowed(IEnumerable<TEnum> allowedEnumValues, IReadOnlyDictionary<TEnum, string> enumToTextMap) {
        return new DataParameterEnumInfo<TEnum>(allowedEnumValues.Distinct(), enumToTextMap);
    }
}