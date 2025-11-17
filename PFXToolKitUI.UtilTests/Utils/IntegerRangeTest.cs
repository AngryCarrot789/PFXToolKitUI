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

using JetBrains.Annotations;
using PFXToolKitUI.Utils.Ranges;
using Xunit;

namespace PFXToolKitUI.UtilTests.Utils;

[TestSubject(typeof(IntegerSet<>))]
public class IntegerRangeTest {
    [Fact]
    public void TestRangeContainsEmptyIsTrue() {
        IntegerRange<int> a = new IntegerRange<int>(0, 20);
        IntegerRange<int> empty = new IntegerRange<int>(10, 10);

        Assert.True(a.Contains(empty));
        Assert.True(empty.Contains(empty));
    }

    [Fact]
    public void TestEmptyContainsRangeIsFalse() {
        IntegerRange<int> empty = new IntegerRange<int>(10, 10);
        IntegerRange<int> a = new IntegerRange<int>(0, 20);

        Assert.False(empty.Contains(a));
    }

    [Theory]
    [InlineData(0, 10, 2, 5)]
    [InlineData(2, 8, 3, 7)]
    [InlineData(-10, 10, -5, 5)]
    public void TestIsFullyContained(int aStart, int aEnd, int bStart, int bEnd) {
        IntegerRange<int> a = new IntegerRange<int>(aStart, aEnd);
        IntegerRange<int> b = new IntegerRange<int>(bStart, bEnd);

        Assert.True(a.Contains(b));
    }

    [Theory]
    [InlineData(0, 10, -1, 5)]
    [InlineData(0, 10, 2, 11)]
    [InlineData(2, 8, 0, 8)]
    public void TestIsNotFullyContained(int aStart, int aEnd, int bStart, int bEnd) {
        IntegerRange<int> a = new IntegerRange<int>(aStart, aEnd);
        IntegerRange<int> b = new IntegerRange<int>(bStart, bEnd);

        Assert.False(a.Contains(b));
    }

    [Fact]
    public void TestOverlappingEmpty() {
        IntegerRange<int> a = new IntegerRange<int>(0, 10);
        IntegerRange<int> empty = new IntegerRange<int>(5, 5);

        Assert.False(a.Overlaps(empty));
        Assert.False(empty.Overlaps(a));
        Assert.False(empty.Overlaps(empty));
    }

    [Theory]
    [InlineData(0, 10, 5, 15)]
    [InlineData(5, 15, 0, 10)]
    [InlineData(0, 10, 0, 10)]
    [InlineData(0, 10, 2, 8)]
    [InlineData(-10, -5, -7, -3)]
    public void TestOverlapping(int aStart, int aEnd, int bStart, int bEnd) {
        IntegerRange<int> a = new IntegerRange<int>(aStart, aEnd);
        IntegerRange<int> b = new IntegerRange<int>(bStart, bEnd);

        Assert.True(a.Overlaps(b));
        Assert.True(b.Overlaps(a));
    }

    [Theory]
    [InlineData(0, 10, 10, 20)]
    [InlineData(10, 20, 0, 10)]
    [InlineData(0, 10, 11, 20)]
    [InlineData(11, 20, 0, 10)]
    public void TestNoOverlapping(int aStart, int aEnd, int bStart, int bEnd) {
        IntegerRange<int> a = new IntegerRange<int>(aStart, aEnd);
        IntegerRange<int> b = new IntegerRange<int>(bStart, bEnd);

        Assert.False(a.Overlaps(b));
        Assert.False(b.Overlaps(a));
    }

    [Fact]
    public void TestOverlappingMinMax() {
        IntegerRange<int> a = new IntegerRange<int>(int.MaxValue - 10, int.MaxValue);
        IntegerRange<int> b = new IntegerRange<int>(int.MaxValue - 5, int.MaxValue);

        Assert.True(a.Overlaps(b));
    }

    [Fact]
    public void TestNoOverlapOnBoundaries() {
        IntegerRange<int> a = new IntegerRange<int>(0, 10);
        IntegerRange<int> b = new IntegerRange<int>(10, 20);

        Assert.False(a.Overlaps(b));
    }
}