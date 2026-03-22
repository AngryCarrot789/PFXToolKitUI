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
using PFXToolKitUI.Utils;
using Xunit;

namespace PFXToolKitUI.UtilTests.Utils;

[TestSubject(typeof(Maths))]
public class MathsTest {
    [Fact]
    public void int_NoOverflow_PositiveNumbers() {
        Assert.Equal(30, Maths.AddClamped(10, 20));
    }

    [Fact]
    public void int_NoOverflow_NegativeNumbers() {
        Assert.Equal(-75, Maths.AddClamped(-50, -25));
        Assert.Equal(-75, Maths.SubClamped(-50, 25));
    }

    [Fact]
    public void int_PositiveOverflow_ClampToMax() {
        Assert.Equal(int.MaxValue, Maths.AddClamped(int.MaxValue - 1, 10));
    }

    [Fact]
    public void int_NegativeOverflow_ClampToMin() {
        Assert.Equal(int.MinValue, Maths.AddClamped(int.MinValue + 5, -100));
        Assert.Equal(int.MinValue, Maths.SubClamped(int.MinValue + 5, 100));
    }

    [Fact]
    public void int_Same() {
        Assert.Equal(123, Maths.AddClamped(123, 0));
        Assert.Equal(-999, Maths.AddClamped(-999, 0));
    }

    [Fact]
    public void int_NoOverflow() {
        Assert.Equal(0, Maths.AddClamped(10, -10));
        Assert.Equal(5, Maths.AddClamped(10, -5));
    }
    
    [Fact]
    public void uint_PositiveOverflow() =>
        Assert.Equal(uint.MaxValue, Maths.AddClamped(uint.MaxValue - 3, (uint) 100));

    [Fact]
    public void uint_NoOverflow() =>
        Assert.Equal(150u, Maths.AddClamped(100u, 50u));
}