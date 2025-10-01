// 
// Copyright (c) 2025-2025 REghZy
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

using System.Collections.Generic;
using JetBrains.Annotations;
using PFXToolKitUI.Utils;
using Xunit;

namespace PFXToolKitUI.UtilTests.Utils;

[TestSubject(typeof(ULongRangeUnion))]
public class ULongRangeUnionTest {
    [Fact]
    public void TestGeneralUsage() {
        ULongRangeUnion list = new ULongRangeUnion();
        list.Add(1);
        list.Add(4);
        list.Add(ULongRange.FromLength(2, 2));
        list.Add(40);
        list.Add(5);
        list.Add(9);
        list.Add(7);
        list.Add(6);
        list.Add(8);
        list.Add(42);
        list.Add(41);

        List<ULongRange> tmpList = list.ToList();
        Assert.Equal(2, tmpList.Count);
        Assert.True(tmpList[0].Equals(new ULongRange(1, 10)));
        Assert.True(tmpList[1].Equals(new ULongRange(40, 43)));

        list.Remove(2);
        tmpList = list.ToList();
        Assert.Equal(3, tmpList.Count);
        Assert.True(tmpList[0].Equals(new ULongRange(1, 2)));
        Assert.True(tmpList[1].Equals(new ULongRange(3, 10)));
        Assert.True(tmpList[2].Equals(new ULongRange(40, 43)));

        list.Remove(new ULongRange(3, 5));
        tmpList = list.ToList();
        Assert.Equal(3, tmpList.Count);
        Assert.True(tmpList[0].Equals(new ULongRange(1, 2)));
        Assert.True(tmpList[1].Equals(new ULongRange(5, 10)));
        Assert.True(tmpList[2].Equals(new ULongRange(40, 43)));
    }

    [Fact]
    public void Test_Not_Present_Zero_To_Max() {
        ULongRangeUnion union = new ULongRangeUnion();
        union.Add(ULongRange.FromLength(5, 11)); // 5 to 15, both inclusive 
        union.Add(ULongRange.FromLength(17, 4)); // 17 to 20, both inclusive

        ULongRangeUnion missing = union.GetPresenceUnion(ULongRange.FromLength(0, ulong.MaxValue), false);
        Assert.True(missing.IsSuperSet(new ULongRange(0, 5)));
        Assert.True(missing.IsSuperSet(new ULongRange(16, 17)));
        Assert.True(missing.IsSuperSet(new ULongRange(21, ulong.MaxValue)));

        Assert.False(missing.IsSuperSet(ULongRange.FromLength(5, 11)));
        Assert.False(missing.IsSuperSet(ULongRange.FromLength(17, 4)));
    }

    [Fact]
    public void Test_Is_Present_Zero_To_Max() {
        ULongRangeUnion union = new ULongRangeUnion();
        union.Add(ULongRange.FromLength(5, 11)); // 5 to 15, both inclusive 
        union.Add(ULongRange.FromLength(17, 4)); // 17 to 20, both inclusive

        ULongRangeUnion missing = union.GetPresenceUnion(ULongRange.FromLength(0, ulong.MaxValue), true);
        Assert.False(missing.IsSuperSet(new ULongRange(0, 5)));
        Assert.False(missing.IsSuperSet(new ULongRange(16, 17)));
        Assert.False(missing.IsSuperSet(new ULongRange(21, ulong.MaxValue)));

        Assert.True(missing.IsSuperSet(ULongRange.FromLength(5, 11)));
        Assert.True(missing.IsSuperSet(ULongRange.FromLength(17, 4)));
    }
    
    [Fact]
    public void Test_Is_Present_Ranged() {
        ULongRangeUnion union = new ULongRangeUnion();
        union.Add(ULongRange.FromLength(5, 11)); // 5 to 15, both inclusive 
        union.Add(ULongRange.FromLength(17, 4)); // 17 to 20, both inclusive

        ULongRangeUnion missing = union.GetPresenceUnion(new ULongRange(8, 19), true); // 8 -> 18
        Assert.True(!missing.Overlaps(new ULongRange(0, 8)));
        Assert.True(missing.IsSuperSet(new ULongRange(8, 16)));
        Assert.True(!missing.Overlaps(new ULongRange(16, 17)));
        Assert.True(missing.IsSuperSet(new ULongRange(17, 19)));
        Assert.True(!missing.Overlaps(new ULongRange(19, ulong.MaxValue)));
    }
    
    [Fact]
    public void Test_Not_Present_Ranged() {
        ULongRangeUnion union = new ULongRangeUnion();
        union.Add(ULongRange.FromLength(5, 11)); // 5 to 15, both inclusive 
        union.Add(ULongRange.FromLength(17, 4)); // 17 to 20, both inclusive

        ULongRangeUnion missing = union.GetPresenceUnion(new ULongRange(8, 19), false); // 8 -> 18
        Assert.True(!missing.Overlaps(new ULongRange(0, 8)));
        Assert.True(!missing.IsSuperSet(new ULongRange(8, 16)));
        Assert.True(missing.Overlaps(new ULongRange(16, 17)));
        Assert.True(!missing.IsSuperSet(new ULongRange(17, 19)));
        Assert.True(!missing.Overlaps(new ULongRange(19, ulong.MaxValue)));
    }
}