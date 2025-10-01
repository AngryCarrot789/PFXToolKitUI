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

[TestSubject(typeof(LongRangeUnion))]
public class LongRangeUnionTest {
    [Fact]
    public void TestGeneralUsage() {
        LongRangeUnion list = new LongRangeUnion();
        list.Add(1);
        list.Add(4);
        list.Add(LongRange.FromLength(2, 2));
        list.Add(40);
        list.Add(5);
        list.Add(9);
        list.Add(7);
        list.Add(6);
        list.Add(8);
        list.Add(42);
        list.Add(41);

        List<LongRange> tmpList = list.ToList();
        Assert.Equal(2, tmpList.Count);
        Assert.True(tmpList[0].Equals(new LongRange(1, 10)));
        Assert.True(tmpList[1].Equals(new LongRange(40, 43)));

        list.Remove(2);
        tmpList = list.ToList();
        Assert.Equal(3, tmpList.Count);
        Assert.True(tmpList[0].Equals(new LongRange(1, 2)));
        Assert.True(tmpList[1].Equals(new LongRange(3, 10)));
        Assert.True(tmpList[2].Equals(new LongRange(40, 43)));

        list.Remove(new LongRange(3, 5));
        tmpList = list.ToList();
        Assert.Equal(3, tmpList.Count);
        Assert.True(tmpList[0].Equals(new LongRange(1, 2)));
        Assert.True(tmpList[1].Equals(new LongRange(5, 10)));
        Assert.True(tmpList[2].Equals(new LongRange(40, 43)));
    }

    [Fact]
    public void Test_Not_Present_Zero_To_Max() {
        LongRangeUnion union = new LongRangeUnion();
        union.Add(LongRange.FromLength(5, 11)); // 5 to 15, both inclusive 
        union.Add(LongRange.FromLength(17, 4)); // 17 to 20, both inclusive

        LongRangeUnion missing = union.GetPresenceUnion(LongRange.FromLength(0, long.MaxValue), false);
        Assert.True(missing.IsSuperSet(new LongRange(0, 5)));
        Assert.True(missing.IsSuperSet(new LongRange(16, 17)));
        Assert.True(missing.IsSuperSet(new LongRange(21, long.MaxValue)));

        Assert.False(missing.IsSuperSet(LongRange.FromLength(5, 11)));
        Assert.False(missing.IsSuperSet(LongRange.FromLength(17, 4)));
    }

    [Fact]
    public void Test_Is_Present_Zero_To_Max() {
        LongRangeUnion union = new LongRangeUnion();
        union.Add(LongRange.FromLength(5, 11)); // 5 to 15, both inclusive 
        union.Add(LongRange.FromLength(17, 4)); // 17 to 20, both inclusive

        LongRangeUnion missing = union.GetPresenceUnion(LongRange.FromLength(0, long.MaxValue), true);
        Assert.False(missing.IsSuperSet(new LongRange(0, 5)));
        Assert.False(missing.IsSuperSet(new LongRange(16, 17)));
        Assert.False(missing.IsSuperSet(new LongRange(21, long.MaxValue)));

        Assert.True(missing.IsSuperSet(LongRange.FromLength(5, 11)));
        Assert.True(missing.IsSuperSet(LongRange.FromLength(17, 4)));
    }
    
    [Fact]
    public void Test_Is_Present_Ranged() {
        LongRangeUnion union = new LongRangeUnion();
        union.Add(LongRange.FromLength(5, 11)); // 5 to 15, both inclusive 
        union.Add(LongRange.FromLength(17, 4)); // 17 to 20, both inclusive

        LongRangeUnion missing = union.GetPresenceUnion(new LongRange(8, 19), true); // 8 -> 18
        Assert.True(!missing.Overlaps(new LongRange(0, 8)));
        Assert.True(missing.IsSuperSet(new LongRange(8, 16)));
        Assert.True(!missing.Overlaps(new LongRange(16, 17)));
        Assert.True(missing.IsSuperSet(new LongRange(17, 19)));
        Assert.True(!missing.Overlaps(new LongRange(19, long.MaxValue)));
    }
    
    [Fact]
    public void Test_Not_Present_Ranged() {
        LongRangeUnion union = new LongRangeUnion();
        union.Add(LongRange.FromLength(5, 11)); // 5 to 15, both inclusive 
        union.Add(LongRange.FromLength(17, 4)); // 17 to 20, both inclusive

        LongRangeUnion missing = union.GetPresenceUnion(new LongRange(8, 19), false); // 8 -> 18
        Assert.True(!missing.Overlaps(new LongRange(0, 8)));
        Assert.True(!missing.IsSuperSet(new LongRange(8, 16)));
        Assert.True(missing.Overlaps(new LongRange(16, 17)));
        Assert.True(!missing.IsSuperSet(new LongRange(17, 19)));
        Assert.True(!missing.Overlaps(new LongRange(19, long.MaxValue)));
    }
}