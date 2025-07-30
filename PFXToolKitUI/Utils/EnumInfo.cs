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

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace PFXToolKitUI.Utils;

public static class EnumInfo<TEnum> where TEnum : unmanaged, Enum {
    public static readonly TEnum MinValue, MaxValue;
    public static readonly ReadOnlyCollection<TEnum> EnumValues;
    public static readonly IReadOnlySet<TEnum> EnumValuesSet;
    public static readonly ReadOnlyCollection<TEnum> EnumValuesOrderedByName;

    // ReSharper disable StaticMemberInGenericType
    public static readonly Type UnderlyingType;
    public static readonly bool IsUnsigned;
    public static readonly int PrimitiveTypeIndex;
    // ReSharper restore StaticMemberInGenericType

    public static long GetSignedValue(TEnum value) {
        if (IsUnsigned) {
            throw new InvalidOperationException("Underlying enum type is unsigned");
        }

        switch (PrimitiveTypeIndex) {
            case 1:  return Unsafe.As<TEnum, sbyte>(ref value);
            case 2:  return Unsafe.As<TEnum, short>(ref value);
            case 3:  return Unsafe.As<TEnum, int>(ref value);
            case 4:  return Unsafe.As<TEnum, long>(ref value);
            default: throw new Exception("EnumInfo class uninitialized");
        }
    }
    
    public static ulong GetUnsignedValue(TEnum value) {
        if (!IsUnsigned) {
            throw new InvalidOperationException("Underlying enum type is not unsigned");
        }
        
        switch (PrimitiveTypeIndex) {
            case 1:  return Unsafe.As<TEnum, byte>(ref value);
            case 2:  return Unsafe.As<TEnum, ushort>(ref value);
            case 3:  return Unsafe.As<TEnum, uint>(ref value);
            case 4:  return Unsafe.As<TEnum, ulong>(ref value);
            default: throw new Exception("EnumInfo class uninitialized");
        }
    }

    static EnumInfo() {
        EnumValues = Enum.GetValues<TEnum>().ToList().AsReadOnly();
        EnumValuesSet = new HashSet<TEnum>(EnumValues);
        EnumValuesOrderedByName = EnumValues.OrderBy(x => x.ToString()).ToList().AsReadOnly();

        Type type = UnderlyingType = Enum.GetUnderlyingType(typeof(TEnum));
        IsUnsigned = type == typeof(byte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong);

        if (type == typeof(sbyte) || type == typeof(byte))
            PrimitiveTypeIndex = 1;
        else if (type == typeof(short) || type == typeof(ushort))
            PrimitiveTypeIndex = 2;
        else if (type == typeof(int) || type == typeof(uint))
            PrimitiveTypeIndex = 3;
        else if (type == typeof(long) || type == typeof(ulong))
            PrimitiveTypeIndex = 4;
        
        Debug.Assert(PrimitiveTypeIndex != 0);

        // Apparently float and double types are technically supported but not compilable so this should work for non-reflection maybe
        Debug.Assert(IsUnsigned || type == typeof(sbyte) || type == typeof(short) || type == typeof(int) || type == typeof(long));

        if (EnumValues.Count < 1) {
            MinValue = MaxValue = default;
        }
        else {
            if (IsUnsigned) {
                TEnum value = EnumValues[0];
                ulong val64 = GetUnsignedValue(value);

                TEnum eMin = value, eMax = value;
                ulong iMin = val64, iMax = val64;
                for (int i = 1; i < EnumValues.Count; i++) {
                    value = EnumValues[i];
                    val64 = GetUnsignedValue(value);
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
                TEnum value = EnumValues[0];
                long val64 = GetSignedValue(value);

                TEnum eMin = value, eMax = value;
                long iMin = val64, iMax = val64;
                for (int i = 1; i < EnumValues.Count; i++) {
                    value = EnumValues[i];
                    val64 = GetSignedValue(value);
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
}