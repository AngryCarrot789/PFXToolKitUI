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

[TestSubject(typeof(ArrayUtils))]
public class ArrayUtilsTest {

    [Fact]
    public void TestGeneralUsage() {
        // test array functions
        int[] array = [0, 1, 2, 3, 4, 5, 6, 7, 8];
        
        int[] newArray = ArrayUtils.RemoveAt(array, 0);
        Assert.Equal([1, 2, 3, 4, 5, 6, 7, 8], newArray);
        
        newArray = ArrayUtils.RemoveAt(newArray, newArray.Length - 1);
        Assert.Equal([1, 2, 3, 4, 5, 6, 7], newArray);
        
        newArray = ArrayUtils.RemoveAt(newArray, 3);
        Assert.Equal([1, 2, 3, 5, 6, 7], newArray);
    }
}