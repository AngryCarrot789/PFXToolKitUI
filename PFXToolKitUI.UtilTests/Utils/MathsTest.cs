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
        Assert.Equal(30, Maths.AddAndClampOverflow(10, 20));
    }

    [Fact]
    public void int_NoOverflow_NegativeNumbers() {
        Assert.Equal(-75, Maths.AddAndClampOverflow(-50, -25));
        Assert.Equal(-75, Maths.SubAndClampOverflow(-50, 25));
    }

    [Fact]
    public void int_PositiveOverflow_ClampToMax() {
        Assert.Equal(int.MaxValue, Maths.AddAndClampOverflow(int.MaxValue - 1, 10));
    }

    [Fact]
    public void int_NegativeOverflow_ClampToMin() {
        Assert.Equal(int.MinValue, Maths.AddAndClampOverflow(int.MinValue + 5, -100));
        Assert.Equal(int.MinValue, Maths.SubAndClampOverflow(int.MinValue + 5, 100));
    }

    [Fact]
    public void int_Same() {
        Assert.Equal(123, Maths.AddAndClampOverflow(123, 0));
        Assert.Equal(-999, Maths.AddAndClampOverflow(-999, 0));
    }

    [Fact]
    public void int_NoOverflow() {
        Assert.Equal(0, Maths.AddAndClampOverflow(10, -10));
        Assert.Equal(5, Maths.AddAndClampOverflow(10, -5));
    }
    
    [Fact]
    public void uint_PositiveOverflow() =>
        Assert.Equal(uint.MaxValue, Maths.AddAndClampOverflow(uint.MaxValue - 3, (uint) 100));

    [Fact]
    public void uint_NoOverflow() =>
        Assert.Equal(150u, Maths.AddAndClampOverflow(100u, 50u));
}